CREATE FUNCTION [bll].[ft_IFRS16] (
	@LeaseId INT,
	@YearlyDiscountRate Decimal (19, 6), --new or updated rate
	@PaymentFrequencyUnitId INT,-- new or updated frequency
	@EffectiveDate DATE,-- Date of new/updated conditions
	@Payments dbo.DatedAmountList READONLY, -- The total payment schedule as updated in the current document
	@PriorSchedule dbo.IFRS16Schedule READONLY -- The original copy of the total payment scedule
)
RETURNS @Result TABLE ( -- Model
	PaymentDate DATE PRIMARY KEY, -- Noted Date 0 -- input
	Payment DECIMAL (19, 6), -- Monetary Value 0 -- input
	NetPresentValue DECIMAL (19, 6), -- Noted Amount 0
	OpeningLiability DECIMAL (19, 6), -- Decimal 1
	InterestExpense DECIMAL (19, 6) -- Decimal 2
)
AS BEGIN
	DECLARE @Monthly INT = dal.fn_UnitCode__Id(N'mo');
	DECLARE @MonthBaseAmount FLOAT = dal.fn_Unit__BaseAmount(@Monthly);
	DECLARE @Scale INT = dal.fn_Unit__BaseAmount(@PaymentFrequencyUnitId) / dal.fn_Unit__BaseAmount(@Monthly);	
	DECLARE @UpdatedSchedule dbo.IFRS16Schedule;
	DECLARE @TenancyStartingDate DATE = (
		SELECT MIN(D.[NotedDate])
		FROM dbo.Documents D
		JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
		WHERE DD.[Code] = N'LeaseContract'
		AND D.[State] = 1
		AND [ResourceId] = @LeaseId
	);
	SET @TenancyStartingDate = ISNULL(@TenancyStartingDate, @EffectiveDate)

	INSERT INTO @UpdatedSchedule([PaymentDate], Payment) SELECT AmountDate, Amount FROM @Payments;
	-- calculate the net present value of the curent payments
	UPDATE @UpdatedSchedule
	SET
		-- Recalculate the Present Value
		[NetPresentValue] = ROUND(ISNULL(dbo.fn_PV (@TenancyStartingDate, @YearlyDiscountRate / 100,
											[PaymentDate], [Payment],
											dal.fn_Unit__BaseAmount(@PaymentFrequencyUnitId)/@MonthBaseAmount), -- will always divide by 12
									0), 2);

	-- Calculate the starting liability back from the original contract by subtracting from Total PV the PV of payments before starting
	-- This is the same as Total PV of payments after starting
	DECLARE @StartingLiability DECIMAL (19, 6) =
		(SELECT SUM([NetPresentValue]) FROM @UpdatedSchedule WHERE [PaymentDate] >= @TenancyStartingDate);

	-- Focus on the 
	DECLARE @Date DATE = (SELECT MIN([PaymentDate]) FROM @UpdatedSchedule WHERE [PaymentDate] >= @TenancyStartingDate);
	DECLARE @ClosingLiability DECIMAL (19, 6) = ISNULL(@StartingLiability, 0);
	WHILE @Date IS NOT NULL
	BEGIN
		UPDATE @UpdatedSchedule
		SET 
			[OpeningLiability] = @ClosingLiability,
			[InterestExpense] = (@YearlyDiscountRate * @Scale / 1200.0) * (@ClosingLiability - [Payment]) -- @Interest on liability a/t payment
		WHERE [PaymentDate] = @Date;
		-- closing liability of the ith date = Starting liability + total interest expenses - total payments
		SET @ClosingLiability  = @StartingLiability + ISNULL((SELECT SUM([InterestExpense] - [Payment]) FROM @UpdatedSchedule WHERE [PaymentDate] <= @Date), 0);	
		SET @Date = (SELECT MIN([PaymentDate]) FROM @UpdatedSchedule WHERE [PaymentDate] > @Date)
	END

	UPDATE @UpdatedSchedule
	SET
		[NetPresentValue] = ROUND([NetPresentValue], 2),
		[OpeningLiability] = ROUND([OpeningLiability], 2),
		[InterestExpense] = ROUND([InterestExpense], 2);

	INSERT INTO @Result SELECT * FROM @PriorSchedule WHERE [PaymentDate] < @EffectiveDate;
	INSERT INTO @Result SELECT * FROM @UpdatedSchedule WHERE [PaymentDate] >= @EffectiveDate;
	
	-- Recall from the prior schedule the payments that are scheduled to happen after the new change date
	MERGE @Result AS t
	USING (SELECT * FROM @PriorSchedule) AS s
	ON (t.PaymentDate = s.PaymentDate)
	-- Turn the @Results table into a differential table. So that, @Result PLUS @Prior are now the new result
	WHEN MATCHED THEN
		UPDATE SET
		t.[Payment] = t.[Payment] - s.[Payment],
		t.[NetPresentValue] = t.[NetPresentValue] - s.[NetPresentValue],
		t.[OpeningLiability] = t.[OpeningLiability] - s.[OpeningLiability],
		t.[InterestExpense] = t.[InterestExpense] - s.[InterestExpense]
	WHEN NOT MATCHED THEN
		INSERT ([PaymentDate], [Payment], [NetPresentValue], [OpeningLiability], [InterestExpense])
		VALUES(s.[PaymentDate], -s.[Payment], -s.[NetPresentValue], -s.[OpeningLiability], -s.[InterestExpense]);

	RETURN
END
GO