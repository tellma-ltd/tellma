CREATE FUNCTION bll.[fn_EOMONTH_ET](@Date DATE) RETURNS DATE
AS
BEGIN

	DECLARE @Year INT = [dbo].[fn_Ethiopian_DatePart]('y', @Date);
	DECLARE @Month INT = [dbo].[fn_Ethiopian_DatePart]('m', @Date);
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
