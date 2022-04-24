CREATE FUNCTION [dal].[fi_ParentAccountType__ChartAnomaliesCurrent]
( --
	@ParentAccountTypeId INT
)
RETURNS TABLE AS RETURN
(
	WITH ActiveAccounts AS (
		SELECT A.[Code], A.[Name], A.[AgentDefinitionId], A.[ResourceDefinitionId], A.[NotedAgentDefinitionId], A.[NotedResourceDefinitionId],
			A.[AgentId], A.[ResourceId], A.[NotedAgentId], A.[NotedResourceId], A.[CenterId]
		FROM dbo.Accounts A
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN dbo.AccountTypes ACP ON AC.[Node].IsDescendantOf(ACP.[Node]) = 1.
		WHERE ACp.[Id] = @ParentAccountTypeId
		AND A.IsActive = 1 -- AND A.[Id] IN (SELECT DISTINCT [AccountId] FROM dbo.Entries)
	)
	SELECT	AA1.[Code] AS [Code1], AA1.[Name] AS [Name1], AA1.[AgentDefinitionId] AS [AD1], AA1.[ResourceDefinitionId] AS [RD1],
			AA1.[NotedAgentDefinitionId] AS [NAD1], AA1.[NotedResourceDefinitionId] AS [NRD1], AA1.[CenterId] AS [C1],
			AA1.[AgentId] AS A1, AA1.[ResourceId] AS [R1], AA1.[NotedAgentId] AS NA1, AA1.[NotedResourceId] AS NR1,
			AA2.[Code] AS [Code2], AA2.[Name] AS [Name2], AA2.[AgentDefinitionId] AS [AD2], AA2.[ResourceDefinitionId] AS [RD2],
			AA2.[NotedAgentDefinitionId] AS [NAD2], AA2.[NotedResourceDefinitionId] AS [NRD2], AA2.[CenterId] AS [C2],
			AA2.[AgentId] AS A2, AA2.[ResourceId] AS [R2], AA2.[NotedAgentId] AS NA2, AA2.[NotedResourceId] AS NR2
	FROM ActiveAccounts AA1
	JOIN ActiveAccounts AA2
	ON  (AA1.[Code] < AA2.[Code])
	AND	(AA1.[AgentDefinitionId] = AA2.[AgentDefinitionId] OR AA1.[AgentDefinitionId] IS NULL AND AA2.[AgentDefinitionId] IS NULL)
	AND (AA1.[ResourceDefinitionId] = AA2.[ResourceDefinitionId] OR AA1.[ResourceDefinitionId] IS NULL AND AA2.[ResourceDefinitionId] IS NULL)
	AND (AA1.[NotedAgentDefinitionId] = AA2.[NotedAgentDefinitionId] OR AA1.[NotedAgentDefinitionId] IS NULL AND AA2.[NotedAgentDefinitionId] IS NULL)
	AND (AA1.[NotedResourceDefinitionId] = AA2.[NotedResourceDefinitionId] OR AA1.[NotedResourceDefinitionId] IS NULL AND AA2.[NotedResourceDefinitionId] IS NULL)
	AND (AA1.[AgentId] = AA2.[AgentId] OR AA1.[AgentId] IS NULL AND AA2.[AgentId] IS NULL)
	AND (AA1.[ResourceId] = AA2.[ResourceId] OR AA1.[ResourceId] IS NULL AND AA2.[ResourceId] IS NULL)
	AND (AA1.[NotedAgentId] = AA2.[NotedAgentId] OR AA1.[NotedAgentId] IS NULL AND AA2.[NotedAgentId] IS NULL)
	AND (AA1.[NotedResourceId] = AA2.[NotedResourceId] OR AA1.[NotedResourceId] IS NULL AND AA2.[NotedResourceId] IS NULL)
	AND (AA1.[CenterId] = AA2.[CenterId] OR AA1.[CenterId] IS NULL AND AA2.[CenterId] IS NULL)
)
GO