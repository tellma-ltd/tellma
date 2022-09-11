CREATE PROCEDURE [dal].[ContractAmendmentTermination_LineKeys__Update]
	@ContractLineDefinitionId INT,
	@ContractAmendmentLineDefinitionId INT,
	@ContractTerminationLineDefinitionId INT,
	@EntryIndex INT
AS
SET @ContractAmendmentLineDefinitionId = ISNULL(@ContractAmendmentLineDefinitionId, 0);
SET @ContractTerminationLineDefinitionId = ISNULL(@ContractTerminationLineDefinitionId, 0);

MERGE INTO dbo.[LineDefinitionLineKeys] AS t
USING (
	SELECT DISTINCT @ContractLineDefinitionId AS [LineDefinitionId], @EntryIndex As [EntryIndex], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId]
	FROM dbo.Lines L
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	WHERE L.[DefinitionId] IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId, @ContractTerminationLineDefinitionId)
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
WHERE L.DefinitionId				IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId, @ContractTerminationLineDefinitionId)
AND T.[LineDefinitionId]			= @ContractLineDefinitionId 
AND E.[Index]						= @EntryIndex 
AND (L.[LineKey] IS NULL OR L.[LineKey] <> T.[Id]);

GO