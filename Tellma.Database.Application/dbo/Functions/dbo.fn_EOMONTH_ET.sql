CREATE FUNCTION [dbo].[fn_EOMONTH_ET](@Date DATE) RETURNS DATE
AS
BEGIN

	DECLARE @Year INT = [dbo].[fn_Ethiopian_DatePart]('y', @Date);
	DECLARE @Month INT = [dbo].[fn_Ethiopian_DatePart]('m', @Date);

	IF @Month = 12 SET @Month = 13; -- Added by MA, 2021.11.19
	DECLARE @Day INT = IIF(
		@Month < 13,
		30 /* !Pagume */,
		IIF(
			@Year % 4 = 3,
			6 /* Pagume & Leap */,
			5 /* Pagume & !Leap */
		)
	);

	RETURN [dbo].[fn_Ethiopian_DateFromParts](@Year, @Month, @Day);
END
GO