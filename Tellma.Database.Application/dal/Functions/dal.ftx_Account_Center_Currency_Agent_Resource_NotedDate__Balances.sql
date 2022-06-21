CREATE FUNCTION [dal].[ft_Account_Center_Currency_Agent_Resource_NotedDate__Balances]
(-- A null parameter means: the matching entry column must be null
	@AccountId INT,
	@CenterId INT,
	@CurrencyId NCHAR (3),
	@AgentId INT,
	@ResourceId INT,
	@NotedDate DATE,
	@AsOf DATE
)
RETURNS @ResultTable TABLE (
	[Quantity] DECIMAL (19, 4),
	[MonetaryValue] DECIMAL (19, 4),
	[Value] DECIMAL (19, 4),
	[NotedAmount] DECIMAL (19, 4)
)
AS BEGIN

	INSERT INTO @ResultTable([Quantity], [MonetaryValue], [Value], [NotedAmount])
	SELECT 
		SUM(E.[Direction] * E.[Quantity]) AS [Quantity],
		SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue],
		SUM(E.[Direction] * E.[Value]) AS [Value],
		SUM(E.[Direction] * E.[NotedAmount]) AS [NotedAmount]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	WHERE L.[State] = 4
	AND (E.[AccountId] = @AccountId)
	AND (E.[CenterId] = @CenterId)
	AND (E.[CurrencyId] = @CurrencyId)
	AND (@AgentId IS NULL AND E.[AgentId] IS NULL OR
			E.[AgentId] = @AgentId)
	AND (@ResourceId IS NULL AND E.[ResourceId] IS NULL	OR
			E.[ResourceId] = @ResourceId)
	AND (@NotedDate IS NULL AND E.[NotedDate] IS NULL OR
			E.[NotedDate] = @NotedDate)	
	AND (@AsOf IS NULL OR L.[PostingDate] <= @AsOf)

	RETURN
END