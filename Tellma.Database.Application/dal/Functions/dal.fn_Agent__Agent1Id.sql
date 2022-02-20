CREATE FUNCTION [dal].[fn_AgentAgent1_Currency__Id] (
	@Agent1Id INT,
	@CurrencyId NCHAR(3)
)
RETURNS INT
AS
BEGIN
	RETURN 	(
		SELECT TOP 1 [Id] FROM dbo.Agents
		WHERE [Agent1Id] = @Agent1Id
		AND [CurrencyId] = @CurrencyId
	)
END