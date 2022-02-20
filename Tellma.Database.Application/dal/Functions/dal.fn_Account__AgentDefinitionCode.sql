CREATE FUNCTION [dal].[fn_Account__AgentDefinitionCode] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Code] FROM [dbo].AgentDefinitions
		WHERE [Id] = (
			SELECT [AgentDefinitionId]
			FROM dbo.Accounts
			WHERE [Id] = @Id
		)
	)
END