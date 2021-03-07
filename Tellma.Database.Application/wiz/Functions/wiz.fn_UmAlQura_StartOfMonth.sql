CREATE FUNCTION [wiz].[fn_UmAlQura_StartOfMonth]
(
	@Date DATE
)
RETURNS DATE
AS
BEGIN
	DECLARE @Year INT = [wiz].[fn_UmAlQura_DatePart]('y', @Date);
	DECLARE @Month INT = [wiz].[fn_UmAlQura_DatePart]('m', @Date);

	RETURN [wiz].[fn_UmAlQura_DateFromParts](@Year, @Month, 1);
END
