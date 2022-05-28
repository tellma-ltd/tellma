CREATE FUNCTION [dal].[fn_LineDefinition_Agent_Resource_Currency__Id]
(
	@LineDefinitionId INT,
	@AgentId INT,
	@ResourceId INT,
	@CurrencyId NCHAR (3)
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT [Id]
		FROM dbo.LineDefinitionsAgentsResourcesCurrencies
		WHERE [LineDefinitionId] = @LineDefinitionId
		AND [AgentId] = @AgentId
		AND [ResourceId] = @ResourceId
		AND [CurrencyId] = @CurrencyId
	)
END
