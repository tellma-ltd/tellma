CREATE PROCEDURE [wiz].[ExpensesByNature__Capitalize]
/*
	[wiz].[ExpensesByNature__Capitalize] @BusinessUnitId = 4, -- import
	@CenterType = N'CurrentInventoriesInTransitExpendituresControl', @ToDate = N'2021-08-06'

	[wiz].[ExpensesByNature__Capitalize] @BusinessUnitId = 6, -- Manufacturing
	@CenterType = N'WorkInProgressExpendituresControl', @ToDate = N'2020-10-10'

*/
	@DocumentIndex	INT = 0,
	@BusinessUnitId INT,
	@CenterType		NVARCHAR (255),
--	@FromDate		DATE,
	@ToDate			DATE
AS
	DECLARE @BusinessUnitNode HIERARCHYID = (SELECT [Node] FROM dbo.[Centers] WHERE [Id] = @BusinessUnitId);
	DECLARE @BSAccountTypeId INT =
		CASE
			WHEN @CenterType = N'ConstructionInProgressExpendituresControl' THEN
				(SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ConstructionInProgress')
			WHEN @CenterType = N'InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl' THEN
				(SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentPropertyUnderConstructionOrDevelopment')
			WHEN @CenterType = N'WorkInProgressExpendituresControl' THEN
				(SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'WorkInProgress')
			WHEN @CenterType = N'CurrentInventoriesInTransitExpendituresControl' THEN
				(SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentInventoriesInTransit')
		END;
	DECLARE @LineDefinitionId INT =
		CASE
			WHEN @CenterType = N'ConstructionInProgressExpendituresControl' THEN
				(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CIPFromConstructionExpense')
			WHEN @CenterType = N'InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl' THEN
				(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IPUCDFromDevelopmentExpense')
			WHEN @CenterType = N'WorkInProgressExpendituresControl' THEN
				(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'WIPFromProductionExpense')
			WHEN @CenterType = N'CurrentInventoriesInTransitExpendituresControl' THEN
				(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IITFromTransitExpense')
		END;

	DECLARE @WideLines WideLineList;

	WITH ExpenseByNatureAccounts AS (
		SELECT [Id] FROM dbo.Accounts
		WHERE AccountTypeId IN (
			SELECT [Id] FROM dbo.AccountTypes
			WHERE [CenterType] = N'Expenditure'
		)
	),
	ActiveCustodies AS (
		SELECT E.[AccountId], E.[CenterId], E.[CustodyId], SUM(E.[Direction] * E.[Value]) AS TotalValue
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
		JOIN dbo.Centers C ON E.[CenterId] = C.[Id]
		WHERE L.[State] = 4
		AND L.PostingDate <= @ToDate
		AND A.AccountTypeId = @BSAccountTypeId
		AND C.[Node].IsDescendantOf(@BusinessUnitNode) = 1
		GROUP BY E.[AccountId], E.[CenterId], E.[CustodyId]
	),
	UnCapitalizedExpenses AS (
		SELECT MIN(E.[Id]) AS [Id], E.[AccountId], E.[CenterId], E.[CustodyId], E.[ResourceId],
				E.[UnitId], SUM(E.[Direction] * E.[Quantity]) AS [Quantity],
				E.[CurrencyId], SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue],
				SUM(E.[Direction] * E.[Value])  AS [Value]
		FROM Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
		JOIN ActiveCustodies C ON E.[CenterId] = C.[CenterId] 	
		WHERE DD.DocumentType = 2 -- event
		AND L.[State] = 4
		AND E.[AccountId] IN (SELECT [Id] FROM ExpenseByNatureAccounts)
		--AND (@FromDate IS NULL OR L.PostingDate >= @FromDate)
		AND (@ToDate IS NULL OR L.PostingDate <= @ToDate)
		AND (E.[CustodyId] IS NULL OR E.[CustodyId] = C.[CustodyId])
		GROUP BY E.[AccountId],  E.[CenterId], E.[CustodyId], E.[ResourceId], E.[UnitId], E.[CurrencyId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	),
--	select * from UnCapitalizedExpenses
	TargetResources AS (
		SELECT E.[AccountId], E.[CenterId], E.[CustodyId], E.[ResourceId], SUM(E.[Direction] * E.[Value]) AS NetValue
		FROM map.DetailsEntries() E
		JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
		JOIN ActiveCustodies C ON E.[AccountId] = C.[AccountId] AND E.[CenterId] = C.[CenterId]
		WHERE DD.DocumentType = 2 -- event
		AND L.[State] = 4
		AND L.[PostingDate] <= @ToDate
		AND A.[AccountTypeId] = @BSAccountTypeId
		AND (E.[CustodyId] IS NULL OR E.[CustodyId] = C.[CustodyId])
		GROUP BY E.[AccountId], E.[CenterId], E.[CustodyId], E.[ResourceId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	),	
	--select * from TargetResources
	ExpenseDistribution AS (
		SELECT U.[Id], U.[AccountId] AS [AccountId1], U.[ResourceId] AS [ResourceId1],
				NULL AS [ParticipantId1], U.[UnitId] AS [UnitId1],
				U.[Quantity] * T.[NetValue] / C.[TotalValue] AS [Quantiy1],
				U.[CurrencyId] AS [Currencyid1],
				U.[MonetaryValue] * T.[NetValue] / C.[TotalValue] AS [MonetaryValue1],
				U.[Value] * T.[NetValue] / C.[TotalValue] AS [Value1],
				T.[ResourceId] AS [ResourceId0], 0 AS [Quantity0],
				R.[UnitId] AS [UnitId0], C.[AccountId] AS [AccountId0],
				U.[CenterId] AS [CenterId0], U.[CenterId] AS [CenterId1],
				U.[CustodyId] AS [CustodyId0], U.[CustodyId] AS [CustodyId1]
		FROM UnCapitalizedExpenses U
		JOIN ActiveCustodies C ON U.[CenterId] = C.[CenterId]
		JOIN TargetResources T ON U.[CenterId] = T.[CenterId]
		JOIN dbo.Resources R ON R.[Id] = T.[ResourceId]
		WHERE (U.[CustodyId] IS NULL OR U.[CustodyId] = C.[CustodyId] AND U.[CustodyId] = T.[CustodyId])
	)
--	select * from ExpenseDistribution
	INSERT INTO @WideLines([Index], [DefinitionId],
			[DocumentIndex],[AccountId0], [CenterId0], [CustodyId0], [ResourceId0],[Quantity0],[UnitId0],
			[AccountId1], [CenterId1], [CustodyId1], [ResourceId1], [ParticipantId1], [CurrencyId1],
			[MonetaryValue1], [Value1])
	SELECT	ROW_NUMBER() OVER(ORDER BY C.[Code], A.[Code], [ResourceId0]) - 1, @LineDefinitionId,
			@DocumentIndex,[AccountId0], [CenterId0], [CustodyId0], [ResourceId0], [Quantity0],[UnitId0],
			[AccountId1], [CenterId1], [CustodyId1], [ResourceId1], [ParticipantId1], [CurrencyId1],
			[MonetaryValue1], [Value1]
	FROM ExpenseDistribution ED
	LEFT JOIN dbo.Custodies C ON ED.[CustodyId1] = C.[Id]
	JOIN dbo.Accounts A ON ED.[AccountId1] = A.[Id]
	WHERE ROUND([MonetaryValue1], 2) > 0 OR ROUND([Value1], 2) > 0

	SELECT * FROM @WideLines;