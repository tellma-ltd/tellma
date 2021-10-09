CREATE FUNCTION [dal].[fn_ResourceDefinition_Code__Id] (
	@ResourceDefinitionCode NVARCHAR (255),
	@ResourceCode NVARCHAR (50)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Id] FROM [dbo].[Resources]
		WHERE [DefinitionId] = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = @ResourceDefinitionCode)
		AND [Code] = @ResourceCode
	)
END