CREATE FUNCTION [dbo].[fn_Ethiopian_DatePart]
(
	@DatePart CHAR (1), -- 'y', 'q', 'm' or 'd'
	@Date DATETIME
)
RETURNS INT
AS
BEGIN
	-- Get the Julian Date Number and use it to calculate the various Ethiopian date parts
	-- http://www.geez.org/Calendars/

	DECLARE @Jdn INT = [dbo].[fn_JulianDayNumber](@Date);
	DECLARE @JdOffset INT = 1723856;
	DECLARE @R INT = (@Jdn - @JdOffset) % 1461;
	DECLARE @N INT = (@R % 365) + 365 * (@R / 1460);

	RETURN 
	(CASE @DatePart
		WHEN 'y' THEN 4 * ((@Jdn - @JdOffset) / 1461) + (@R / 365) - (@R / 1460)
		WHEN 'q' THEN 
			CASE -- 1 + ((Month - 1) / 3)
				WHEN 1 + (@N / 90) >= 4 THEN 4 -- Pagume is still Q4 not Q5
				ELSE 1 + (@N / 90) 
			END
		WHEN 'm' THEN (@N / 30) + 1
		WHEN 'd' THEN (@N % 30) + 1
	END)
END;