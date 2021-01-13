CREATE FUNCTION [wiz].[fn_Ethiopian_DatePart]
(
	@DatePart NVARCHAR (15),
	@Date DATETIME
)
RETURNS INT
AS
BEGIN
	-- Get the Julian Date Number and use it to calculate the various Ethiopian date parts
	-- http://www.geez.org/Calendars/

	DECLARE @Jdn INT = [wiz].[fn_JulianDayNumber](@Date);
	DECLARE @JdOffset INT = 1723856;
	DECLARE @R INT = (@Jdn - @JdOffset) % 1461;
	DECLARE @N INT = (@R % 365) + 365 * (@R / 1460);

	RETURN 
	(CASE @DatePart
		WHEN N'year' THEN 4 * ((@Jdn - @JdOffset) / 1461) + (@R / 365) - (@R / 1460)
		WHEN N'quarter' THEN 
			CASE -- 1 + ((Month - 1) / 3)
				WHEN 1 + (@N / 90) >= 4 THEN 4 -- Pagume is still Q4 not Q5
				ELSE 1 + (@N / 90) 
			END
		WHEN N'month' THEN (@N / 30) + 1
		WHEN N'day' THEN (@N % 30) + 1
	END)
END;