CREATE FUNCTION [dbo].[fn_UmAlQura_StartOfMonth]
(
	@Date DATE
)
RETURNS DATE
AS
BEGIN
	DECLARE @Year INT = [dbo].[fn_UmAlQura_DatePart]('y', @Date);
	DECLARE @Month INT = [dbo].[fn_UmAlQura_DatePart]('m', @Date);

	RETURN [dbo].[fn_UmAlQura_DateFromParts](@Year, @Month, 1);
END
