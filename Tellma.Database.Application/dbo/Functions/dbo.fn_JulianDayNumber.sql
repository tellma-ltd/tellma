CREATE FUNCTION [dbo].[fn_JulianDayNumber] -- can be inlined
(
	@Date DATE
)
RETURNS INT
AS
BEGIN
	-- This function calculates the Julian Day Number (JDN) based on a gregorian date
	-- The JDN is a popular intermediary format for conversion from one calendar to another
	-- https://quasar.as.utexas.edu/BillInfo/JulianDatesG.html

	DECLARE @D INT = DATEPART(DAY, @Date);
	DECLARE @M INT = DATEPART(MONTH, @Date);
	DECLARE @Y INT = DATEPART(YEAR, @Date);

	IF (@M <= 2) -- Jan or Feb
	BEGIN
		SET @Y = @Y - 1;
		SET @M = @M + 12;
	END;

	DECLARE @A DECIMAL = FLOOR(@Y / 100);
	DECLARE @B DECIMAL = FLOOR(@A / 4);
	DECLARE @C DECIMAL = 2 - @A + @B;
	DECLARE @E DECIMAL = FLOOR(365.25 * (@Y + 4716));
	DECLARE @F DECIMAL = FLOOR(30.6001 * (@M + 1));

	DECLARE @Jdn DECIMAL = @C + @D + @E + @F - 1524;

	RETURN CAST (@Jdn AS INT);
END;