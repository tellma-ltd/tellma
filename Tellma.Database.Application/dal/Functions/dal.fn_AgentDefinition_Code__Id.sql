CREATE FUNCTION [dal].[fn_AgentDefinition_Code__Id] (
	@AgentDefinitionCode NVARCHAR (255),
	@AgentCode NVARCHAR (50)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT [Id] FROM [dbo].[Agents]
		WHERE [DefinitionId] = (SELECT [Id] FROM dbo.AgentDefinitions WHERE [Code] = @AgentDefinitionCode)
		AND [Code] = @AgentCode
	)
END