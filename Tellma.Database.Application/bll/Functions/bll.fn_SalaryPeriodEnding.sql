CREATE FUNCTION bll.[fn_SalaryPeriodEnding](@Time2 DATE)
RETURNS DATE
AS
BEGIN
	DECLARE @ShortCompanyName NVARCHAR (255) = (SELECT [ShortCompanyName] FROM dbo.Settings) ;
	DECLARE @FirstDayOfPeriod TINYINT = bll.fn_SalaryPeriodFirstDay();

	RETURN
		IIF(
			@FirstDayOfPeriod > 1,
			
			DATEADD(MONTH,
				IIF(DAY(@Time2) < @FirstDayOfPeriod, 0, +1),			
					DATEFROMPARTS( -- Take the first day of the next cycle
						YEAR(@Time2),
						MONTH(@Time2),
						@FirstDayOfPeriod - 1)
			),
			
			DATEADD(DAY, -- Backtrack by 1 day
					-1,
					DATEADD(MONTH, -- Go Next Month
							1,
							DATEFROMPARTS( -- Take the first day of the month
								YEAR(@Time2),
								MONTH(@Time2),
								@FirstDayOfPeriod)
					)
			)
		)
END