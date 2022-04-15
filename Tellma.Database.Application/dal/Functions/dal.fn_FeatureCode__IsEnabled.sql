CREATE FUNCTION [dal].[fn_FeatureCode__IsEnabled]
(
	@FeatureCode NVARCHAR (255)
)
RETURNS BIT
AS
BEGIN
	RETURN (SELECT [IsEnabled] FROM dbo.FeatureFlags WHERE [FeatureCode] = @FeatureCode)
END