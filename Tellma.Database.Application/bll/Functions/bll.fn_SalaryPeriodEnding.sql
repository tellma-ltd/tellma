CREATE FUNCTION bll.[fn_SalaryPeriodEnding](@Time2 DATE)
RETURNS DATE
AS
BEGIN
	DECLARE @FirstDayOfPeriod TINYINT = 25;

	RETURN
		DATEADD(DAY, -- Backtrack by 1 day
				-1,
				DATEADD(MONTH, -- Go Next Month
						IIF(DAY(@Time2) >= @FirstDayOfPeriod, +2, +1),
						DATEFROMPARTS( -- Take the first day of the month
							YEAR(@Time2),
							MONTH(@Time2),
							1)
				)
		)
END