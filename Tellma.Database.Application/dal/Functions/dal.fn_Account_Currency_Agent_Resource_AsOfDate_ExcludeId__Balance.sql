CREATE FUNCTION [dal].[fn_Account_Currency_Agent_Resource_AsOfDate_ExcludeId__Balance] (
	@AccountId	INT,
	@CurrencyId	NCHAR (3),
	@AgentId	INT,
	@ResourceId	INT,
	@AsOfDate	DATE,
	@ExcludeDocumentId INT
)
RETURNS DECIMAL (19, 4)
AS
BEGIN
	RETURN (
		--
		SELECT SUM(IIF(
					[State] = 4 OR ([State] < 4 AND [MonetaryValue]*[Direction] < 0),
					[Direction] * [MonetaryValue],
					0)
		)
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.DefinitionId
		WHERE LD.[LineType] = 100
		AND  L.[State]		>= 0
		AND (E.[AccountId]	= @AccountId)
		AND (E.[CurrencyId]	= @CurrencyId)
		AND (E.[AgentId]	= @AgentId		OR E.AgentId IS NULL AND @AgentId IS NULL)
		AND (E.[ResourceId]	= @ResourceId	OR E.[ResourceId] IS NULL AND @ResourceId IS NULL)
		AND L.[PostingDate] <=@AsOfDate
		AND L.[DocumentId]  <>@ExcludeDocumentId
	)
END