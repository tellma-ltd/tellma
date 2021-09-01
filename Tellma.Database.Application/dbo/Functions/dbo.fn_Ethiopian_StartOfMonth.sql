CREATE FUNCTION [dbo].[fn_Ethiopian_StartOfMonth]
(
	@Date DATE
)
RETURNS DATE
AS
BEGIN
	DECLARE @Year INT = [dbo].[fn_Ethiopian_DatePart]('y', @Date);
	DECLARE @Month INT = [dbo].[fn_Ethiopian_DatePart]('m', @Date);

	RETURN [dbo].[fn_Ethiopian_DateFromParts](@Year, @Month, 1);
END
