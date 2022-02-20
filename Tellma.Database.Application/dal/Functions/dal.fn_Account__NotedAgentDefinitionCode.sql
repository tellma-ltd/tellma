CREATE FUNCTION [dal].[fn_Account__NotedAgentDefinitionCode] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Code] FROM [dbo].AgentDefinitions
		WHERE [Id] = (
			SELECT [NotedAgentDefinitionId]
			FROM dbo.Accounts
			WHERE [Id] = @Id
		)
	)
END