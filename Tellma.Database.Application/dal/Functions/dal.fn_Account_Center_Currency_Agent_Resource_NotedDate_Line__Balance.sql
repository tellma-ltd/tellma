CREATE FUNCTION [dal].[fn_Account_Center_Currency_Agent_Resource_NotedDate_Line__Balance]
(
	@AccountId	INT,
	@CenterId	INT,
	@CurrencyId	NCHAR (3),
	@AgentId	INT,
	@ResourceId	INT,
	@NotedDate	DATE,
	@LineId INT
)
RETURNS DECIMAL (19, 4)
AS
BEGIN
	RETURN (
		SELECT SUM([Direction] * [MonetaryValue])
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		WHERE L.[State]		= 4
		AND (E.[AccountId]	= @AccountId)
		AND (E.[CenterId]	= @CenterId)
		AND (E.[CurrencyId]	= @CurrencyId)
		AND (E.[AgentId]	= @AgentId		OR E.AgentId IS NULL AND @AgentId IS NULL)
		AND (E.[ResourceId]	= @ResourceId	OR E.[ResourceId] IS NULL AND @ResourceId IS NULL)
		AND (E.[NotedDate]	<= @NotedDate	OR E.[NotedDate] IS NULL)-- AND @NotedDate IS NULL)
		AND (E.[LineId] <> @LineId)
	)
END
GO