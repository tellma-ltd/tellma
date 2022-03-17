CREATE PROCEDURE [wiz].[ExpensesByNature__CapitalizeToIIT]
/* 4: Import, 6: Manufacturing, 74: Adama, 75: AA
	[wiz].[ExpensesByNature__CapitalizeToIIT] @BusinessUnitId = 4, @ToDate = N'2021-08-06'
*/
	@DocumentIndex	INT = 0,
	@BusinessUnitId INT = 4,
	@ToDate			DATE = N'2021-06-07',
	@AbstractSupplierCode NVARCHAR (50) = N'0'
AS
	DECLARE @BSAccountTypeConcept NVARCHAR (255) = N'CurrentInventoriesInTransit';
	DECLARE @BusinessUnitNode HIERARCHYID = (SELECT [Node] FROM dbo.[Centers] WHERE [Id] = @BusinessUnitId);
	DECLARE @BSAccountTypeId INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = @BSAccountTypeConcept);
	DECLARE @CenterType NVARCHAR (255) = @BSAccountTypeConcept + N'ExpendituresControl';
	DECLARE @AbstractSupplierId INT = dal.fn_AgentDefinition_Code__Id(N'Supplier', @AbstractSupplierCode);
	DECLARE @EntryTypeId INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionsFromPurchasesInventoriesExtension')
	DECLARE @OpeningBalanceEntryTypeId INT = 
		(SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = 'OpeningBalancesInventoriesExtension');
	DECLARE @LineDefinitionId INT =	(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IITFromTransitExpense');
	DECLARE @WideLines [WidelineList];

	DECLARE @ExpenseByNatureNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'ExpenseByNature');
	WITH ExpenseByNatureAccounts AS (
		SELECT [Id] FROM dbo.Accounts
		WHERE AccountTypeId IN (
			SELECT [Id] FROM dbo.AccountTypes
			WHERE [Node].IsDescendantOf(@ExpenseByNatureNode) = 1
		)
	),
	UnCapitalizedExpenses AS (
		SELECT MIN(E.[Id]) AS [Id], E.[AccountId], E.[CenterId], E.[AgentId],
				E.[ResourceId], E.[UnitId], SUM(E.[Direction] * E.[Quantity]) AS [Quantity],
				E.[CurrencyId], SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue],
				SUM(E.[Direction] * E.[Value])  AS [Value]--, ISNULL(E.[NotedAgentId], @AbstractSupplierId) AS [NotedAgentId]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Centers C ON E.[CenterId] = C.[Id]
		WHERE L.[State] = 4
		AND E.[AccountId] IN (SELECT [Id] FROM ExpenseByNatureAccounts)
		AND C.CenterType = @CenterType
		AND C.[Node].IsDescendantOf(@BusinessUnitNode) = 1
		AND L.PostingDate <= @ToDate
		GROUP BY E.[AccountId],  E.[CenterId], E.[AgentId], E.[ResourceId], E.[UnitId], E.[CurrencyId]--, ISNULL(E.[NotedAgentId], @AbstractSupplierId)
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	),--select * from UnCapitalizedExpenses
	TargetResources AS (
		SELECT E.[AccountId], E.[AgentId],
			E.[ResourceId], SUM(E.[Direction] * E.[BaseQuantity]) AS NetQuantity, SUM(E.[Direction] * E.[Value]) AS NetValue
		FROM map.DetailsEntries() E
		JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN UnCapitalizedExpenses UE ON E.[AgentId] = UE.[AgentId]
		WHERE L.[State] = 4
		AND A.[AccountTypeId] = @BSAccountTypeId
		AND E.[CenterId] = @BusinessUnitId
		AND E.EntryTypeId IN (@EntryTypeId, @OpeningBalanceEntryTypeId) -- . Works for IIT, to allocate the expenses over the resources
		GROUP BY E.[AccountId], E.[AgentId], E.[ResourceId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	), -- select * from TargetResources
	ActiveAgents AS (
		SELECT [AccountId], [AgentId], SUM([NetQuantity]) AS [TotalQuantity], SUM([NetValue]) AS TotalValue
		FROM TargetResources
		GROUP BY [AccountId], [AgentId]
	),
	ExpenseDistribution AS (
		SELECT 
			AR.AccountId AS [AccountId0], @BusinessUnitId AS [CenterId0], U.[AgentId] AS [AgentId0],
			T.[ResourceId] AS [ResourceId0], 0 AS [Quantity0], R.[UnitId] AS [UnitId0], 
			U.[AccountId] AS [AccountId1], U.[CenterId] AS [CenterId1],  U.[AgentId] AS [AgentId1],
			U.[ResourceId] AS [ResourceId1], U.[Quantity] * T.[NetValue] / AR.[TotalValue] AS [Quantity1], U.[UnitId] AS [UnitId1],
			@AbstractSupplierId AS [NotedAgentId1], U.[CurrencyId] AS [CurrencyId1],
			U.[MonetaryValue] * T.[NetValue] / AR.[TotalValue] AS [MonetaryValue1],
			U.[Value] * T.[NetValue] / AR.[TotalValue] AS [Value1]
		FROM UnCapitalizedExpenses U
		JOIN ActiveAgents AR ON U.[AgentId] = AR.[AgentId]
		JOIN TargetResources T ON AR.[AccountId] = T.[AccountId] AND U.[AgentId] = T.[AgentId]
		JOIN dbo.Resources R ON R.[Id] = T.[ResourceId]
	) -- select * from ExpenseDistribution
	INSERT INTO @WideLines([Index], [DefinitionId],
			[DocumentIndex], [AccountId0], [CenterId0], [AgentId0], [ResourceId0],[Quantity0],[UnitId0],
			[AccountId1], [CenterId1], [AgentId1], [ResourceId1], [Quantity1], [UnitId1], [NotedAgentId1], [CurrencyId1],
			[MonetaryValue1], [Value1])
	SELECT	ROW_NUMBER() OVER(ORDER BY ED.[AgentId1], A.[Code], [ResourceId0]) - 1, @LineDefinitionId,
			@DocumentIndex,[AccountId0], [CenterId0], [AgentId0], [ResourceId0], [Quantity0], [UnitId0],
			[AccountId1], [CenterId1], [AgentId1], [ResourceId1], [Quantity1], [UnitId1], [NotedAgentId1], [CurrencyId1],
			[MonetaryValue1], [Value1]
	FROM ExpenseDistribution ED
	JOIN dbo.Accounts A ON ED.[AccountId1] = A.[Id]
	WHERE [MonetaryValue1] > 0.005 OR [Value1] > 0.005;

	SELECT * FROM @WideLines;