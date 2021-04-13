CREATE PROCEDURE [wiz].[ExpensesByNature__Capitalize]
/*
	[wiz].[ExpensesByNature__Capitalize] @BusinessUnitId = 4, -- import
	@CenterType = N'CurrentInventoriesInTransitExpendituresControl', @FromDate = N'2020-08-07', @ToDate = N'2020-09-10'
*/
	@DocumentIndex	INT = 0,
	@BusinessUnitId INT,
	@LineDefinitionId INT,
	@FromDate		DATE,
	@ToDate			DATE
AS
	DECLARE @BusinessUnitNode HIERARCHYID = (SELECT [Node] FROM dbo.[Centers] WHERE [Id] = @BusinessUnitId);

	DECLARE @BSAccountTypeId INT =
		CASE
			WHEN @LineDefinitionId = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CIPFromConstructionExpense') THEN
				(SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ConstructionInProgress')
			WHEN  @LineDefinitionId = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IPUCDFromDevelopmentExpense') THEN
				(SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentPropertyUnderConstructionOrDevelopment')
			WHEN  @LineDefinitionId = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'WIPFromProductionExpense') THEN
				(SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'WorkInProgress')
			WHEN  @LineDefinitionId = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IITFromTransitExpense') THEN
				(SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentInventoriesInTransit')
		END;

	DECLARE @WideLines WideLineList;
	DECLARE @ExpenseByNatureNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'ExpenseByNature');
	WITH ExpenseByNatureAccounts AS (
		SELECT [Id] FROM dbo.Accounts
		WHERE AccountTypeId IN (
			SELECT [Id] FROM dbo.AccountTypes
			WHERE [Node].IsDescendantOf(@ExpenseByNatureNode) = 1
		)
	),
	ActiveCenters AS (
		SELECT E.[CenterId], SUM(E.[Direction] * E.[Value]) AS TotalValue
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
		JOIN dbo.Centers C ON E.[CenterId] = C.[Id]
		WHERE L.[State] = 4
		AND L.PostingDate <= @ToDate
		AND A.AccountTypeId = @BSAccountTypeId
		AND C.[Node].IsDescendantOf(@BusinessUnitNode) = 1
		GROUP BY E.[CenterId]
		HAVING SUM(E.[Direction] * E.[Value]) > 0
	),
	UnCapitalizedExpenses AS (
		SELECT MIN(E.[Id]) AS [Id], E.[AccountId], E.[CenterId], E.[ResourceId],
				E.[UnitId], SUM(E.[Direction] * E.[Quantity]) AS [Quantity],
				E.[CurrencyId], SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue],
				SUM(E.[Direction] * E.[Value])  AS [Value]
		FROM Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
		JOIN ActiveCenters C ON E.[CenterId] = C.[CenterId]
		WHERE DD.DocumentType = 2 -- event
		AND L.[State] = 4
		AND E.[AccountId] IN (SELECT [Id] FROM ExpenseByNatureAccounts)
		AND (@FromDate IS NULL OR L.PostingDate >= @FromDate)
		AND (@ToDate IS NULL OR L.PostingDate <= @ToDate)
		GROUP BY E.[AccountId],  E.[CenterId], E.[ResourceId], E.[UnitId], E.[CurrencyId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	),
	TargetResources AS (
		SELECT E.[CenterId], E.[ResourceId], SUM(E.[Direction] * E.[Value]) AS NetValue
		FROM map.DetailsEntries() E
		JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
		JOIN ActiveCenters C ON E.[CenterId] = C.[CenterId]
		WHERE DD.DocumentType = 2 -- event
		AND L.[State] = 4
		AND L.[PostingDate] <= @ToDate
		AND A.[AccountTypeId] = @BSAccountTypeId
		GROUP BY E.[CenterId], E.[ResourceId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	), --	select * from TargetResources
	ExpenseDistribution AS (
		SELECT U.[Id], U.[AccountId] AS [AccountId1], U.[ResourceId] AS [ResourceId1],
				NULL AS [NotedRelationId1], U.[UnitId] AS [UnitId1],
				U.[Quantity] * T.[NetValue] / C.[TotalValue] AS [Quantiy1],
				U.[CurrencyId] AS [Currencyid1],
				U.[MonetaryValue] * T.[NetValue] / C.[TotalValue] AS [MonetaryValue1],
				U.[Value] * T.[NetValue] / C.[TotalValue] AS [Value1],
				T.[ResourceId] AS [ResourceId0], 0 AS [Quantity0],
				R.[UnitId] AS [UnitId0],
				U.[CenterId] AS [CenterId0], U.[CenterId] AS [CenterId1]
		FROM UnCapitalizedExpenses U
		JOIN ActiveCenters C ON U.[CenterId] = C.[CenterId]
		JOIN TargetResources T ON U.[CenterId] = T.[CenterId]
		JOIN dbo.Resources R ON R.[Id] = T.[ResourceId]
	)
	INSERT INTO @WideLines([Index], [DefinitionId],
			[DocumentIndex],[CenterId0], [ResourceId0],[Quantity0],[UnitId0],
			[AccountId1], [CenterId1], [ResourceId1], [NotedRelationId1], [CurrencyId1],
			[MonetaryValue1], [Value1])
	SELECT	ROW_NUMBER() OVER(ORDER BY C.[Code], A.[Code], [ResourceId0]) - 1, @LineDefinitionId,
			@DocumentIndex,[CenterId0], [ResourceId0], [Quantity0],[UnitId0],
			[AccountId1], [CenterId1], [ResourceId1], [NotedRelationId1], [CurrencyId1],
			[MonetaryValue1], [Value1]
	FROM ExpenseDistribution ED
	JOIN dbo.Centers C ON ED.[CenterId1] = C.[Id]
	JOIN dbo.Accounts A ON ED.[AccountId1] = A.[Id]

	SELECT * FROM @WideLines;