CREATE PROCEDURE [wiz].[ExpensesByNature__CapitalizeToCIP]
-- TODO: Verify the logic make sense when we have production batches. Do we have to write a separate SProc?
	@DocumentIndex	INT = 0,
	@BusinessUnitId INT,
	@ToDate			DATE,
	@AbstractSupplierCode NVARCHAR (50) = N'0'
AS
	DECLARE @BSAccountTypeConcept NVARCHAR (255) = N'ConstructionInProgress'
	DECLARE @BusinessUnitNode HIERARCHYID = (SELECT [Node] FROM dbo.[Centers] WHERE [Id] = @BusinessUnitId);

DECLARE @BSAccountTypeId INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = @BSAccountTypeConcept);

	DECLARE @CenterType NVARCHAR (255) = @BSAccountTypeConcept + N'ExpendituresControl';

	DECLARE @AbstractSupplierId INT = dal.fn_AgentDefinition_Code__Id(N'Supplier', @AbstractSupplierCode);

	DECLARE @LineDefinitionId INT =
			(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ConstructionInProgressExpense');

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
				ISNULL(E.[ResourceId], -1) AS ResourceID, E.[UnitId], SUM(E.[Direction] * E.[Quantity]) AS [Quantity],
				E.[CurrencyId], SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue],
				SUM(E.[Direction] * E.[Value])  AS [Value]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Centers C ON E.[CenterId] = C.[Id]
		WHERE L.[State] = 4
		AND E.[AccountId] IN (SELECT [Id] FROM ExpenseByNatureAccounts)
		AND C.CenterType = @CenterType
		AND C.[Node].IsDescendantOf(@BusinessUnitNode) = 1
		AND L.PostingDate <= @ToDate
		GROUP BY E.[AccountId],  E.[CenterId], E.[AgentId], E.[ResourceId], E.[UnitId], E.[CurrencyId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	),--	select * from UnCapitalizedExpenses
	TargetResources AS (
		SELECT E.[AccountId],
			@BusinessUnitId AS [CenterId],
			E.[ResourceId], 
			SUM(E.[Direction] * E.[BaseQuantity]) AS NetQuantity, SUM(E.[Direction] * E.[Value]) AS NetValue
		FROM map.DetailsEntries() E
		JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		WHERE L.[State] = 4
		AND A.[AccountTypeId] = @BSAccountTypeId
		AND A.[ResourceId] IS NOT NULL -- MA: 2022-11-05 to exclude the deferred expense account
		AND E.[CenterId] = @BusinessUnitId
		GROUP BY E.[AccountId], E.[AgentId], E.[ResourceId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	), -- 	select * from TargetResources
	ExpenseDistribution AS (
		SELECT 
			T.AccountId AS [AccountId0], @BusinessUnitId AS [CenterId0], NULL AS [AgentId0],
			T.[ResourceId] AS [ResourceId0], 0 AS [Quantity0], R.[UnitId] AS [UnitId0], 
			U.[AccountId] AS [AccountId1], U.[CenterId] AS [CenterId1],  U.[AgentId] AS [AgentId1],
			U.[ResourceId] AS [ResourceId1], U.[Quantity] AS [Quantity1],
			U.[UnitId] AS [UnitId1],
			@AbstractSupplierId AS [NotedAgentId1], U.[CurrencyId] AS [CurrencyId1],
			U.[MonetaryValue] AS [MonetaryValue1], 
			U.[Value] AS [Value1]
		FROM UnCapitalizedExpenses U
		JOIN dbo.Centers C ON C.[Id] = U.[CenterId]
		JOIN TargetResources T ON T.[CenterId] = C.[ParentId]
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

	UPDATE @WideLines SET [ResourceId1] = NULL WHERE [ResourceId1] = -1;
	SELECT * FROM @WideLines;