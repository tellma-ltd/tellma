CREATE FUNCTION [dbo].[fn_FromJulianDayNumber]
(
	@Jdn INT
)
RETURNS DATE
AS
BEGIN
	-- This function calculates the Gregorian Date from the Julian Day Number (JDN)
	-- The JDN is a popular intermediary format for conversion from one calendar to another
	-- https://quasar.as.utexas.edu/BillInfo/JulianDatesG.html

	DECLARE @Z DECIMAL = @Jdn;
	DECLARE @W DECIMAL = FLOOR((@Z - 1867216.25) / 36524.25)
	DECLARE @X DECIMAL = FLOOR(@W / 4.0);
    DECLARE @A DECIMAL = @Z + 1 + @W - @X;
    DECLARE @B DECIMAL = @A + 1524.0;
	DECLARE @C DECIMAL = FLOOR((@B - 122.1) / 365.25);
	DECLARE @D DECIMAL = FLOOR(365.25 * @C);
	DECLARE @E DECIMAL = FLOOR((@B - @D) / 30.6001);
	DECLARE @F DECIMAL = FLOOR(30.6001 * @E);
	
	DECLARE @Day DECIMAL = @B - @D - @F;
	DECLARE @Month DECIMAL = CASE WHEN @E <= 13 THEN @E - 1 ELSE @E - 13 END;
	DECLARE @Year DECIMAL = CASE WHEN @Month <= 2 THEN @C - 4715 ELSE @C - 4716 END;

	RETURN DATEFROMPARTS(@Year, @Month, @Day);
END;