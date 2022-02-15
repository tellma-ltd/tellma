CREATE FUNCTION [dal].[fn_AgentDefinitionCode__Id] (
	@Code NVARCHAR (255)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Id] FROM dbo.AgentDefinitions
		WHERE [Code] = @Code
	)
END