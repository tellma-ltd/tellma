CREATE PROCEDURE [dal].[ContractAndAmendment_LineKeys__Update]
	@EmployeeBenefitInCash INT,
	@EmployeeBenefitInCashAmended INT,
	@EntryIndex INT
AS
SET @EmployeeBenefitInCashAmended = ISNULL(@EmployeeBenefitInCashAmended, 0);
MERGE INTO dbo.[LineDefinitionLineKeys] AS t
USING (
	SELECT DISTINCT @EmployeeBenefitInCash AS [LineDefinitionId], @EntryIndex As [EntryIndex], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId]
	FROM dbo.Lines L
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	WHERE L.[DefinitionId] IN (@EmployeeBenefitInCash, @EmployeeBenefitInCashAmended)
	AND E.[Index] = @EntryIndex
) AS s ON (
		t.[LineDefinitionId]			= s.[LineDefinitionId]
	AND	t.[CenterId]					= s.[CenterId]
	AND	t.[CurrencyId]					= s.[CurrencyId]
	AND ISNULL(t.[AgentId], -1)			= ISNULL(s.[AgentId], -1)
	AND ISNULL(t.[ResourceId], -1)		= ISNULL(s.[ResourceId], -1)
	AND ISNULL(t.[NotedAgentId], -1)	= ISNULL(s.[NotedAgentId], -1)
	AND ISNULL(t.[NotedResourceId], -1)	= ISNULL(s.[NotedResourceId], -1)
)
WHEN NOT MATCHED THEN
INSERT ([LineDefinitionId],		[EntryIndex], [CenterId],	[CurrencyId],	[AgentId], [ResourceId],	[NotedAgentId],		[NotedResourceId])
VALUES (s.[LineDefinitionId], s.[EntryIndex], s.[CenterId], s.[CurrencyId], s.[AgentId], s.[ResourceId], s.[NotedAgentId], s.[NotedResourceId]);

/*
-- I commented out the following code as it was hanging. I replaced it with the following code. MA 2022.06.24
WITH VLines AS (
	SELECT L.[LineKey], @EmployeeBenefitInCash AS [LineDefinitionId], @EntryIndex As [EntryIndex], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId]
	FROM dbo.Lines L
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	WHERE L.[DefinitionId] IN (@EmployeeBenefitInCash, @EmployeeBenefitInCashAmended)
	AND E.[Index] = @EntryIndex
)
UPDATE V
SET [LineKey] = T.[Id]
FROM VLines V
JOIN [LineDefinitionLineKeys] T
ON T.[LineDefinitionId]				= V.[LineDefinitionId]
AND	T.[CenterId]					= V.[CenterId]
AND	T.[CurrencyId]					= V.[CurrencyId]
AND ISNULL(T.[AgentId], -1)			= ISNULL(V.[AgentId], -1)
AND ISNULL(T.[ResourceId], -1)		= ISNULL(V.[ResourceId], -1)
AND ISNULL(T.[NotedAgentId], -1)	= ISNULL(V.[NotedAgentId], -1)
AND ISNULL(T.[NotedResourceId], -1)	= ISNULL(V.[NotedResourceId], -1)
WHERE [LineKey] IS NULL OR [LineKey] <> T.[Id];
*/

UPDATE L
SET L.[LineKey] = T.[Id]
FROM dbo.Lines L
JOIN dbo.Entries E ON E.[LineId] = L.[Id]
JOIN [LineDefinitionLineKeys] T
ON	T.[CenterId]					= E.[CenterId]
AND	T.[CurrencyId]					= E.[CurrencyId]
AND (T.[AgentId] IS NULL AND E.[AgentId] IS NULL OR T.[AgentId] = E.[AgentId])
AND (T.[ResourceId] IS NULL AND E.[ResourceId] IS NULL OR T.[ResourceId] = E.[ResourceId])
AND (T.[NotedAgentId] IS NULL AND E.[NotedAgentId] IS NULL OR T.[NotedAgentId] = E.[NotedAgentId])
AND (T.NotedResourceId IS NULL AND E.NotedResourceId IS NULL OR T.NotedResourceId = E.NotedResourceId)
WHERE L.DefinitionId				IN (@EmployeeBenefitInCash, @EmployeeBenefitInCashAmended)
AND T.[LineDefinitionId]			= @EmployeeBenefitInCash 
AND E.[Index]						= @EntryIndex 
AND (L.[LineKey] IS NULL OR L.[LineKey] <> T.[Id]);

GO