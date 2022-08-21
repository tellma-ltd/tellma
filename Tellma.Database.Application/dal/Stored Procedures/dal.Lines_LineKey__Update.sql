CREATE PROCEDURE [dal].[Lines_LineKey__Update]
	@ContractLineDefinitionId INT,
	@ContractAmendmentLineDefinitionId INT,
	@ContractTerminationLineDefinitionId INT,

	@EntryIndex INT,
	@Ids IdList READONLY
AS
SET @ContractAmendmentLineDefinitionId = ISNULL(@ContractAmendmentLineDefinitionId, 0);
SET @ContractTerminationLineDefinitionId = ISNULL(@ContractTerminationLineDefinitionId, 0);
MERGE INTO dbo.[LineDefinitionLineKeys] AS t
USING (
	SELECT DISTINCT @ContractLineDefinitionId AS [LineDefinitionId], @EntryIndex As [EntryIndex], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId]
	FROM @Ids FL
	JOIN dbo.Lines L ON L.[Id] = FL.[Id]
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

WITH VLines AS (
	SELECT L.[LineKey], @ContractLineDefinitionId AS [LineDefinitionId], @EntryIndex As [EntryIndex], E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId]
	FROM @Ids FL
	JOIN dbo.Lines L ON L.[Id] = FL.[Id]
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	WHERE L.[DefinitionId] IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId, @ContractTerminationLineDefinitionId)
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
GO