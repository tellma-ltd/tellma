CREATE FUNCTION [dal].[fn_Agent__AgentDefinitionCode] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Code] FROM [dbo].[AgentDefinitions]
		WHERE [Id] = (
			SELECT [DefinitionId] FROM dbo.Agents WHERE Id = @Id
		)
	)
END