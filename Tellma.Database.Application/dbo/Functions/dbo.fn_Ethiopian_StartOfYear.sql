CREATE FUNCTION [dbo].[fn_Ethiopian_StartOfYear]
(
	@Date DATE
)
RETURNS DATE
AS
BEGIN
	DECLARE @Year INT = [dbo].[fn_Ethiopian_DatePart]('y', @Date);

	RETURN [dbo].[fn_Ethiopian_DateFromParts](@Year, 1, 1);
END
