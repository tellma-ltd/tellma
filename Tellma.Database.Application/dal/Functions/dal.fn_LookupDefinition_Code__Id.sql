CREATE FUNCTION [dal].[fn_LookupDefinition_Code__Id] (
	@LookupDefinitionCode NVARCHAR (255),
	@LookupCode NVARCHAR (50)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Id] FROM [dbo].[Lookups]
		WHERE [DefinitionId] = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = @LookupDefinitionCode)
		AND [Code] = @LookupCode
	)
END