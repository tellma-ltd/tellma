CREATE FUNCTION [wiz].[fn_Ethiopian_StartOfYear]
(
	@Date DATE
)
RETURNS DATE
AS
BEGIN
	DECLARE @Year INT = [wiz].[fn_Ethiopian_DatePart]('y', @Date);

	RETURN [wiz].[fn_Ethiopian_DateFromParts](@Year, 1, 1);
END
