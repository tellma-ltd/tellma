CREATE FUNCTION [dal].[fn_ResourceDefinitionCode__Id] (
	@Code NVARCHAR (255)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Id] FROM dbo.ResourceDefinitions
		WHERE [Code] = @Code
	)
END