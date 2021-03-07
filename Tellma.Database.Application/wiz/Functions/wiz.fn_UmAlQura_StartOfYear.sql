CREATE FUNCTION [wiz].[fn_UmAlQura_StartOfYear]
(
	@Date DATE
)
RETURNS DATE
AS
BEGIN
	DECLARE @Year INT = [wiz].[fn_UmAlQura_DatePart]('y', @Date);

	RETURN [wiz].[fn_UmAlQura_DateFromParts](@Year, 1, 1);
END
