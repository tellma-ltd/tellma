CREATE FUNCTION [dbo].[fn_XPV] (
-- switches between PV and XPV depending on dates
	@DiscountRate DECIMAL (19, 6),
	@NumberOfPeriods INT,
	@Payment DECIMAL (19, 6)
) RETURNS DECIMAL (19, 6)
AS BEGIN
    RETURN @Payment / POWER (1 + @DiscountRate/100.0, @NumberOfPeriods);
END
GO