CREATE FUNCTION [dbo].[fn_PV] (
	@AsOfDate DATE = NULL,
    @YearlyDiscountRate DECIMAL (19, 6),
    @ScheduledPaymentDate DATE,  -- Date payment scheduled
    @Payment DECIMAL (19, 6),
	@MonthsBetweenScheduledPayments INT
  
) RETURNS DECIMAL (19, 6)
AS BEGIN
    DECLARE @Periods INT;
    SET @Periods = 1.0 * DATEDIFF(MONTH,
							ISNULL(@AsOfDate, GETDATE()),
							DATEADD(DAY, 1, @ScheduledPaymentDate)) / @MonthsBetweenScheduledPayments;
    RETURN @Payment / POWER (1.0 + @YearlyDiscountRate * @MonthsBetweenScheduledPayments / 12, @Periods);
END
GO