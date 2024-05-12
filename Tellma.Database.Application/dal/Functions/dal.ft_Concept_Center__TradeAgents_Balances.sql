CREATE FUNCTION [dal].[ft_Concept_Center__TradeAgents_Balances]
(
	@ParentConcept NVARCHAR (255),
	@ParentCenterId INT
)
RETURNS @ResultTable TABLE (
	[CenterId] INT,
	[AccountId] INT,
	[CurrencyId] NCHAR (3),
	[AgentId] INT,
	--[ResourceId] INT,
	--[NotedAgentId] INT,
	--[NotedResourceId] INT,
	[NotedDate] DATE,
	[Balance] DECIMAL (19, 4),
	[Value] DECIMAL (19, 4),
	[NotedAmount] DECIMAL (19, 4),
	[EntryTypeId] INT -- MA added 2023.06.23 to better handle trade payables and receivables
)
AS BEGIN
	DECLARE @ParentNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@ParentConcept);
	DECLARE @ParentCenterNode HIERARCHYID = dal.fn_Center__Node(@ParentCenterId);

	INSERT INTO @ResultTable([CenterId], [AccountId], [CurrencyId], [AgentId], --[ResourceId], [NotedAgentId], [NotedResourceId], 
		[NotedDate], [EntryTypeId], [Balance], [Value], [NotedAmount])
	SELECT E.[CenterId], E.[AccountId], E.[CurrencyId], E.[AgentId], --E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId], 
		AG.[ToDate], E.[EntryTypeId], SUM(E.[Direction] * E.[MonetaryValue]), SUM(E.[Direction] * E.[Value]), SUM(E.[Direction] * E.[NotedAmount])
	FROM dbo.Entries E
	JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
	JOIN dbo.Centers C ON C.[Id] = E.[CenterId]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.[Id]
	WHERE L.[State] = 4 -- >= 0
	AND AC.[Node].IsDescendantOf(@ParentNode) = 1
	AND (@ParentCenterId IS NULL OR C.[Node].IsDescendantOf(@ParentCenterNode) = 1)
	GROUP BY E.[CenterId], E.[AccountId], E.[CurrencyId], E.[AgentId], --E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId], 
			AG.[ToDate], E.[EntryTypeId]

	RETURN
END
GO