CREATE FUNCTION [dal].[fn_Account__NotedResourceDefinitionCode] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Code] FROM [dbo].ResourceDefinitions
		WHERE [Id] = (
			SELECT [NotedResourceDefinitionId]
			FROM dbo.Accounts
			WHERE [Id] = @Id
		)
	)
END