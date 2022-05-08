CREATE FUNCTION [dal].[fn_AgentDefinition__CurrencyVisibility] (
	@Id INT
)
RETURNS NVARCHAR (50)
AS
BEGIN
	RETURN 	(
		SELECT [CurrencyVisibility] FROM dbo.AgentDefinitions
		WHERE [Id] = @Id
	)
END