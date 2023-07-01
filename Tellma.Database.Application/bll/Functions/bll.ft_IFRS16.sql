CREATE FUNCTION [bll].[ft_IFRS16] (
	@LeaseId INT,
	@YearlyDiscountRate Decimal (19, 6),
	@PaymentFrequencyUnitId INT,
	@LeaseStartingDate DATE,
	@Payments dbo.DatedAmountList READONLY,
	@PriorSchedule dbo.IFRS16Schedule READONLY
)
RETURNS @Result TABLE ( -- Model
	PaymentDate DATE PRIMARY KEY, -- Noted Date 0
	Payment DECIMAL (19, 6), -- Monetary Value 0
	NetPresentValue DECIMAL (19, 6), -- Noted Amount 0
	OpeningLiability DECIMAL (19, 6), -- Decimal 1
	InterestExpense DECIMAL (19, 6) -- Decimal 2
)
AS BEGIN
	DECLARE @Monthly INT = dal.fn_UnitCode__Id(N'mo');
	DECLARE @MonthBaseAmount FLOAT = dal.fn_Unit__BaseAmount(@Monthly);
	DECLARE @Scale INT = dal.fn_Unit__BaseAmount(@PaymentFrequencyUnitId) / dal.fn_Unit__BaseAmount(@Monthly);	

	INSERT INTO @Result([PaymentDate], Payment) SELECT AmountDate, Amount FROM @Payments;
	UPDATE @Result
	SET
		[NetPresentValue] = ISNULL(dbo.fn_PV (@LeaseStartingDate, @YearlyDiscountRate / 100,
											[PaymentDate], [Payment],
											dal.fn_Unit__BaseAmount(@PaymentFrequencyUnitId)/@MonthBaseAmount),
									0);

	DECLARE @RightOfUse DECIMAL (19, 6) = (SELECT SUM([NetPresentValue]) FROM @Result);

	DECLARE @Date DATE = (SELECT MIN([PaymentDate]) FROM @Result);
	DECLARE @OpeningBalance DECIMAL (19, 6) = ISNULL(@RightOfUse, 0);

	WHILE @Date IS NOT NULL
	BEGIN
		UPDATE @Result
		SET [OpeningLiability] = @OpeningBalance,
			[InterestExpense] = IIF(@Date <= @LeaseStartingDate, 0, 
				(@YearlyDiscountRate * @Scale / 1200.0) * @OpeningBalance) -- @Interest
		WHERE [PaymentDate] = @Date;

		SET @OpeningBalance  = @RightOfUse + ISNULL((SELECT SUM([InterestExpense] - [Payment]) FROM @Result WHERE [PaymentDate] <= @Date), 0);	
		SET @Date = (SELECT MIN([PaymentDate]) FROM @Result WHERE [PaymentDate] > @Date)
	END

	IF @OpeningBalance <> 0
	UPDATE @Result SET OpeningLiability = OpeningLiability - @OpeningBalance WHERE [PaymentDate] = (SELECT MAX([PaymentDate]) FROM @Result);

	UPDATE @Result
	SET
		[NetPresentValue] = ROUND([NetPresentValue], 2),
		[OpeningLiability] = ROUND([OpeningLiability], 2),
		[InterestExpense] = ROUND([InterestExpense], 2)
		
	MERGE @Result AS t
	USING (SELECT * FROM @PriorSchedule WHERE [PaymentDate] >= @LeaseStartingDate) AS s
	ON (t.PaymentDate = s.PaymentDate)
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