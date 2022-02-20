CREATE FUNCTION [dal].[fn_Account__ResourceDefinitionCode] (
	@Id INT
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN 	(
		SELECT [Code] FROM [dbo].ResourceDefinitions
		WHERE [Id] = (
			SELECT [ResourceDefinitionId]
			FROM dbo.Accounts
			WHERE [Id] = @Id
		)
	)
END