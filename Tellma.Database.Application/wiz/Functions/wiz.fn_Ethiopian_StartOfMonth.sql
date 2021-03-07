CREATE FUNCTION [wiz].[fn_Ethiopian_StartOfMonth]
(
	@Date DATE
)
RETURNS DATE
AS
BEGIN
	DECLARE @Year INT = [wiz].[fn_Ethiopian_DatePart]('y', @Date);
	DECLARE @Month INT = [wiz].[fn_Ethiopian_DatePart]('m', @Date);

	RETURN [wiz].[fn_Ethiopian_DateFromParts](@Year, @Month, 1);
END
