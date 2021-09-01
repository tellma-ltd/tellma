CREATE FUNCTION [dbo].[fn_Ethiopian_DateFromParts]
(
	@EtYear INT,
	@EtMonth INT,
	@EtDay INT
)
RETURNS DATE
AS
BEGIN
	DECLARE @JdOffset INT = 1723856;
	DECLARE @Jdn INT = (@JdOffset + 365)
               + 365 * (@EtYear - 1)
               + (@EtYear / 4)
               + 30 * @EtMonth
               + @EtDay - 31;

	RETURN [dbo].[fn_FromJulianDayNumber](@Jdn);
END
