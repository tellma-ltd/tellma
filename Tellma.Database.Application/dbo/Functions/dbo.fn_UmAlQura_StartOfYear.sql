CREATE FUNCTION [dbo].[fn_UmAlQura_StartOfYear]
(
	@Date DATE
)
RETURNS DATE
AS
BEGIN
	DECLARE @Year INT = [dbo].[fn_UmAlQura_DatePart]('y', @Date);

	RETURN [dbo].[fn_UmAlQura_DateFromParts](@Year, 1, 1);
END
