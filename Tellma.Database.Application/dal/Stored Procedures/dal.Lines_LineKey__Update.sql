CREATE PROCEDURE [dal].[Lines_LineKey__Update]
	@ContractLineDefinitionId INT,
	@ContractAmendmentLineDefinitionId INT,
	@ContractTerminationLineDefinitionId INT,
	@EntryIndex INT,
	@Ids IdList READONLY,
	@CenterIsIncluded BIT = 1,
	@CurrencyIsIncluded BIT = 1,
	@AgentIsIncluded BIT = 1,
	@ResourceIsIncluded BIT = 1,
	@NotedAgentIsIncluded BIT = 1,
	@NotedResourceIsIncluded BIT = 1
AS
SET @ContractAmendmentLineDefinitionId = ISNULL(@ContractAmendmentLineDefinitionId, 0);
SET @ContractTerminationLineDefinitionId = ISNULL(@ContractTerminationLineDefinitionId, 0);
DECLARE @OldContractAmendmentLineDefinitionId INT;
DECLARE @FunctionalCurrencyId NCHAR (3) = dal.fn_FunctionalCurrencyId();

IF @ContractAmendmentLineDefinitionId <> 0
BEGIN
	DECLARE @ContractAmendmentLineDefinitionCode NVARCHAR (255) = dal.fn_LineDefinition__Code(@ContractAmendmentLineDefinitionId);
	IF @ContractAmendmentLineDefinitionCode IS NULL THROW 50000, N'New Contract Amendment Version is not deployed', 1;
	SET @OldContractAmendmentLineDefinitionId = ISNULL(dal.fn_LineDefinitionCode__Id(N'(Old)' + @ContractAmendmentLineDefinitionCode), 0);
END

MERGE INTO dbo.[LineDefinitionLineKeys] AS t
USING (
	SELECT DISTINCT @ContractLineDefinitionId AS [LineDefinitionId], @EntryIndex As [EntryIndex],
--		E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId]
		IIF(@CenterIsIncluded = 0, NULL, E.[CenterId]) AS [CenterId],
		IIF(@CurrencyIsIncluded = 0, NULL, E.[CurrencyId]) AS [CurrencyId], 
		IIF(@AgentIsIncluded = 0, NULL, E.[AgentId]) AS [AgentId], 
		IIF(@ResourceIsIncluded = 0, NULL, E.[ResourceId]) AS [ResourceId], 
		IIF(@NotedAgentIsIncluded = 0, NULL, E.[NotedAgentId]) AS [NotedAgentId], 
		IIF(@NotedResourceIsIncluded = 0, NULL, E.[NotedResourceId]) AS [NotedResourceId]
		--, L.[Decimal1] -- MA: Added 2023.04.07, Commented 2025-08-28
	FROM @Ids FL
	JOIN dbo.Lines L ON L.[Id] = FL.[Id]
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	WHERE L.DefinitionId IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId, @ContractTerminationLineDefinitionId,
													@OldContractAmendmentLineDefinitionId)
	AND E.[Index] = @EntryIndex
) AS s ON (
		(t.[LineDefinitionId]			= s.[LineDefinitionId])
	AND	(ISNULL(t.[CenterId], -1)		= ISNULL(s.[CenterId], -1)			OR @CenterIsIncluded = 0)
	AND	(ISNULL(t.[CurrencyId], -1)		= ISNULL(s.[CurrencyId], -1)		OR @CurrencyIsIncluded = 0)
	AND (ISNULL(t.[AgentId], -1)		= ISNULL(s.[AgentId], -1)			OR @AgentIsIncluded = 0)
	AND (ISNULL(t.[ResourceId], -1)		= ISNULL(s.[ResourceId], -1)		OR @ResourceIsIncluded = 0)
	AND (ISNULL(t.[NotedAgentId], -1)	= ISNULL(s.[NotedAgentId], -1)		OR @NotedAgentIsIncluded = 0)
	AND (ISNULL(t.[NotedResourceId], -1)= ISNULL(s.[NotedResourceId], -1)	OR @NotedResourceIsIncluded = 0)
--	AND t.[Decimal1]					= s.[Decimal1] -- MA: Added 2023.04.07, Commented 2025-08-28
)
WHEN NOT MATCHED THEN
INSERT ([LineDefinitionId],		[EntryIndex], [CenterId],	[CurrencyId],	[AgentId], [ResourceId],	[NotedAgentId],		[NotedResourceId])--, [Decimal1]) --  Commented 2025-08-28
VALUES (s.[LineDefinitionId], s.[EntryIndex], s.[CenterId], s.[CurrencyId], s.[AgentId], s.[ResourceId], s.[NotedAgentId], s.[NotedResourceId]);--, s.[Decimal1]); --  Commented 2025-08-28

WITH VLines AS (
	SELECT L.[LineKey], @ContractLineDefinitionId AS [LineDefinitionId], @EntryIndex As [EntryIndex], 
--		E.[CenterId], E.[CurrencyId], E.[AgentId], E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId]
		IIF(@CenterIsIncluded = 0, NULL, E.[CenterId]) AS [CenterId],
		IIF(@CurrencyIsIncluded = 0, NULL, E.[CurrencyId]) AS [CurrencyId], 
		IIF(@AgentIsIncluded = 0, NULL, E.[AgentId]) AS [AgentId], 
		IIF(@ResourceIsIncluded = 0, NULL, E.[ResourceId]) AS [ResourceId], 
		IIF(@NotedAgentIsIncluded = 0, NULL, E.[NotedAgentId]) AS [NotedAgentId], 
		IIF(@NotedResourceIsIncluded = 0, NULL, E.[NotedResourceId]) AS [NotedResourceId]
		--L.[Decimal1] -- MA: Added 2023.04.07  Commented 2025-08-28
	FROM @Ids FL
	JOIN dbo.Lines L ON L.[Id] = FL.[Id]
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	WHERE L.DefinitionId IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId, @ContractTerminationLineDefinitionId,
													@OldContractAmendmentLineDefinitionId)
	AND E.[Index] = @EntryIndex
)
UPDATE V
SET [LineKey] = T.[Id]
FROM VLines V
JOIN [LineDefinitionLineKeys] T
ON T.[LineDefinitionId]				= V.[LineDefinitionId]
AND	(ISNULL(T.[CenterId], -1)		= ISNULL(V.[CenterId], -1)			OR @CenterIsIncluded = 0)
AND	(ISNULL(T.[CurrencyId], -1)		= ISNULL(V.[CurrencyId], -1)		OR @CurrencyIsIncluded = 0)
AND (ISNULL(T.[AgentId], -1)		= ISNULL(V.[AgentId], -1)			OR @AgentIsIncluded = 0)
AND (ISNULL(T.[ResourceId], -1)		= ISNULL(V.[ResourceId], -1)		OR @ResourceIsIncluded = 0)
AND (ISNULL(T.[NotedAgentId], -1)	= ISNULL(V.[NotedAgentId], -1)		OR @NotedAgentIsIncluded = 0)
AND (ISNULL(T.[NotedResourceId], -1)= ISNULL(V.[NotedResourceId], -1)	OR @NotedResourceIsIncluded = 0)
--AND (T.[Decimal1]					= V.[Decimal1])  -- MA: Added 2023.04.07  Commented 2025-08-28
WHERE [LineKey] IS NULL OR [LineKey] <> T.[Id];
GO