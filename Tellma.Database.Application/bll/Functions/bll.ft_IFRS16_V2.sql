CREATE FUNCTION [bll].[ft_IFRS16_V2] (
-- Can handle multiple ROUs
-- Payments can happen anywhere
--	@IFRSModel TINYINT, -- 0: Yearly, 1: Half Yearly, 2: Quarterly, 3: Monthly, 4: Daily
	@Leases LeaseList READONLY,
	@IFRS16Schedules dbo.IFRS16ScheduleList READONLY, -- The total lease schedule as updated in the current document
	@PriorSchedules dbo.IFRS16ScheduleList READONLY -- The original copy of the total lease schedule
)
RETURNS @Result TABLE ( -- Model
	LeaseId	INT,
	PostingDate DATE, -- Posting Date 0 -- input
	Payment DECIMAL (19, 6), -- Monetary Value 0 -- input
--	PaymentAtEndOfDate BIT,
	NumberOfPeriods INT,
	NetPresentValue DECIMAL (19, 6) DEFAULT (0),
	OpeningLiability DECIMAL (19, 6) DEFAULT (0),
	InterestExpense DECIMAL (19, 6) DEFAULT (0),
	PRIMARY KEY (LeaseId, PostingDate)
)
AS BEGIN
	DECLARE @UpdatedSchedules dbo.IFRS16ScheduleList;
	INSERT INTO @UpdatedSchedules([LeaseId], [PostingDate], [Payment], [NumberOfPeriods])
	SELECT [LeaseId], [PostingDate], [Payment], [NumberOfPeriods]
	FROM @IFRS16Schedules;

	-- calculate the net present value of the curent payments
	UPDATE US 
	SET [NetPresentValue] = ROUND(ISNULL(dbo.fn_XPV(L.[DiscountRate], US.[NumberOfPeriods], US.[Payment]), 0), 4)
	FROM @UpdatedSchedules US
	JOIN @Leases L ON L.[LeaseId] = US.[LeaseId];
	
	DECLARE @LeaseId INT = 0;
	WHILE EXISTS(SELECT * FROM @Leases WHERE [LeaseId] > @LeaseId)
	BEGIN
		SELECT @LeaseId =  MIN(LeaseId) FROM @Leases WHERE [LeaseId] > @LeaseId;

		DECLARE @UpdatedStartDate DATE, @DiscountRate DECIMAL (19, 6), @TenancyStartDate DATE;
		SELECT @UpdatedStartDate = [UpdatedStartDate], @DiscountRate = [DiscountRate], @TenancyStartDate = TenancyStartDate
		FROM @Leases
		WHERE [LeaseId] = @LeaseId;
	
		-- Calculate the starting liability back from the original contract by subtracting from Total PV the PV of payments before starting
		-- This is the same as Total PV of payments after starting
		DECLARE @StartingLiability DECIMAL (19, 6) = (
			SELECT SUM(US.[NetPresentValue])
			FROM @UpdatedSchedules US
			JOIN @Leases L ON L.[LeaseId] = US.[LeaseId]
			WHERE US.[LeaseId] = @LeaseId AND US.[PostingDate] >= L.TenancyStartDate);

		-- Focus on the 
		DECLARE @Date DATE = (SELECT MIN([PostingDate]) FROM @UpdatedSchedules WHERE [LeaseId] = @LeaseId AND [PostingDate] >= @TenancyStartDate);
		DECLARE @ClosingLiability DECIMAL (19, 6) = ISNULL(@StartingLiability, 0);
		WHILE @Date IS NOT NULL
		BEGIN
			UPDATE @UpdatedSchedules
			SET 
				[OpeningLiability] = @ClosingLiability,
				[InterestExpense] = @DiscountRate * (@ClosingLiability - [Payment]) / 100 -- @Interest on liability a/t payment
			WHERE [LeaseId] = @LeaseId AND [PostingDate] = @Date;
			-- closing liability of the ith date = Starting liability + total interest expenses - total payments
			SET @ClosingLiability  = @StartingLiability + ISNULL(
					(SELECT SUM([InterestExpense] - [Payment]) FROM @UpdatedSchedules WHERE [LeaseId] = @LeaseId AND [PostingDate] <= @Date),
				0);	
			SET @Date = (SELECT MIN([PostingDate]) FROM @UpdatedSchedules WHERE [LeaseId] = @LeaseId AND [PostingDate] > @Date)
		END
	END
	UPDATE @UpdatedSchedules
	SET
		[NetPresentValue] = ROUND([NetPresentValue], 2),
		[OpeningLiability] = ROUND([OpeningLiability], 2),
		[InterestExpense] = ROUND([InterestExpense], 2);

	INSERT INTO @Result SELECT * FROM @PriorSchedules WHERE [PostingDate] < @UpdatedStartDate;
	INSERT INTO @Result SELECT * FROM @UpdatedSchedules WHERE [PostingDate] >= @UpdatedStartDate;
	
	-- Recall from the prior schedule the payments that are scheduled to happen after the new change date
	MERGE @Result AS t
	USING (SELECT * FROM @PriorSchedules) AS s
	ON (t.[LeaseId] = s.[LeaseId] AND t.PostingDate = s.[PostingDate])
	-- Turn the @Results table into a differential table. So that, @Result PLUS @Prior are now the new result
	WHEN MATCHED THEN
		UPDATE SET
		t.[Payment] = t.[Payment] - s.[Payment],
		t.[NetPresentValue] = t.[NetPresentValue] - s.[NetPresentValue],
		t.[OpeningLiability] = t.[OpeningLiability] - s.[OpeningLiability],
		t.[InterestExpense] = t.[InterestExpense] - s.[InterestExpense]
	WHEN NOT MATCHED THEN
		INSERT ([LeaseId], [PostingDate], [Payment], [NetPresentValue], [OpeningLiability], [InterestExpense])
		VALUES(s.[LeaseId], s.[PostingDate], -s.[Payment], -s.[NetPresentValue], -s.[OpeningLiability], -s.[InterestExpense]);
	
	RETURN
END
GO