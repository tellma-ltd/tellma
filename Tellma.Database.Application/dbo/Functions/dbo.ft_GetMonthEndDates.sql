CREATE FUNCTION [dbo].[ft_GetMonthEndDates]
(
    @StartDate DATE,
    @EndDate DATE
)
RETURNS @MonthEndDates TABLE (MonthEndDate DATE)
AS
BEGIN
    DECLARE @CurrentDate DATE = EOMONTH(@StartDate);

    WHILE @CurrentDate <= @EndDate
    BEGIN
        INSERT INTO @MonthEndDates (MonthEndDate)
        VALUES (@CurrentDate);

        SET @CurrentDate = EOMONTH(DATEADD(MONTH, 1, @CurrentDate));
    END

    RETURN;
END;
GO