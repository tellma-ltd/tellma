﻿CREATE PROCEDURE [wiz].[ExpensesByNature__Capitalize]
/* 4: Import, 6: Manufacturing, 74: Adama, 75: AA
	[wiz].[ExpensesByNature__Capitalize] @BusinessUnitId = 4,
	@CenterType = N'CurrentInventoriesInTransitExpendituresControl', @ToDate = N'2020-08-06'

	[wiz].[ExpensesByNature__Capitalize] @BusinessUnitId = 4,
	@CenterType = N'WorkInProgressExpendituresControl', @ToDate =  @ToDate = N'2021-02-07'

	[wiz].[ExpensesByNature__Capitalize] @BusinessUnitId = 74,
	@CenterType = N'InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl', @ToDate = N'2020-08-06'
*/

	@DocumentIndex	INT = 0,
	@BusinessUnitId INT,
	@BSAccountTypeConcept NVARCHAR (255), --ConstructionInProgress, InvestmentPropertyUnderConstructionOrDevelopment,WorkInProgress,CurrentInventoriesInTransit
	@ToDate			DATE
AS
	DECLARE @BusinessUnitNode HIERARCHYID = (SELECT [Node] FROM dbo.[Centers] WHERE [Id] = @BusinessUnitId);

	DECLARE @BSAccountTypeId INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = @BSAccountTypeConcept);

	DECLARE @EntryTypeId INT = 
		CASE
			WHEN @BSAccountTypeConcept = N'ConstructionInProgress' THEN
				(SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'???')
			WHEN @BSAccountTypeConcept = N'InvestmentPropertyUnderConstructionOrDevelopment' THEN
				(SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'???')
			WHEN @BSAccountTypeConcept = N'WorkInProgress' THEN
				(SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'???')
			WHEN @BSAccountTypeConcept = N'CurrentInventoriesInTransit' THEN
				(SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionsFromPurchasesInventoriesExtension')
		END;

	DECLARE @OpeningBalanceEntryTypeId INT = 
		(SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = 'OpeningBalancesInventoriesExtension');

	DECLARE @LineDefinitionId INT =
		CASE
			WHEN @BSAccountTypeConcept = N'ConstructionInProgress' THEN
				(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CIPFromConstructionExpense')
			WHEN @BSAccountTypeConcept = N'InvestmentPropertyUnderConstructionOrDevelopment' THEN
				(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IPUCDFromDevelopmentExpense')
			WHEN @BSAccountTypeConcept = N'WorkInProgress' THEN
				(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'WIPFromProductionExpense')
			WHEN @BSAccountTypeConcept = N'CurrentInventoriesInTransit' THEN
				(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IITFromTransitExpense')
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
	UnCapitalizedExpenses AS (
		SELECT MIN(E.[Id]) AS [Id], E.[AccountId], E.[CenterId], ISNULL(E.[RelationId], -1) AS RelationId,
				E.[ResourceId], E.[UnitId], SUM(E.[Direction] * E.[Quantity]) AS [Quantity],
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
		GROUP BY E.[AccountId],  E.[CenterId], E.[RelationId], E.[ResourceId], E.[UnitId], E.[CurrencyId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	),--select * from UnCapitalizedExpenses
	TargetResources AS (
		SELECT E.[AccountId], E.[CenterId], ISNULL(E.[RelationId], -1) AS RelationId,
			E.[ResourceId], SUM(E.[Direction] * E.[BaseQuantity]) AS NetQuantity, SUM(E.[Direction] * E.[Value]) AS NetValue
		FROM map.DetailsEntries() E
		JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN UnCapitalizedExpenses C ON
			E.[CenterId] = C.[CenterId]
			AND ISNULL(E.[RelationId], -1) = C.[RelationId]
		WHERE L.[State] = 4
		AND A.[AccountTypeId] = @BSAccountTypeId
		AND E.EntryTypeId IN (@EntryTypeId, @OpeningBalanceEntryTypeId) -- . Works for IIT
		GROUP BY E.[AccountId], E.[CenterId], E.[RelationId], E.[ResourceId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	), -- select * from TargetResources
	ActiveRelations AS (
		SELECT [AccountId], [CenterId], [RelationId], SUM([NetQuantity]) AS [TotalQuantity], SUM([NetValue]) AS TotalValue
		FROM TargetResources
		GROUP BY [AccountId], [CenterId], [RelationId]
	),
	ExpenseDistribution AS (
		SELECT U.[Id], U.[AccountId] AS [AccountId1], U.[ResourceId] AS [ResourceId1],
				NULL AS [NotedRelationId1], U.[UnitId] AS [UnitId1],
				U.[Quantity] * T.[NetValue] / AR.[TotalValue] AS [Quantiy1],
				U.[CurrencyId] AS [Currencyid1],
				U.[MonetaryValue] * T.[NetValue] / AR.[TotalValue] AS [MonetaryValue1],
				U.[Value] * T.[NetValue] / AR.[TotalValue] AS [Value1],
				T.[ResourceId] AS [ResourceId0], 0 AS [Quantity0],
				R.[UnitId] AS [UnitId0], AR.AccountId AS [AccountId0],
				U.[CenterId] AS [CenterId0], U.[CenterId] AS [CenterId1],
				IIF(U.[RelationId] = -1, NULL, U.[RelationId]) AS [RelationId0],
				IIF(U.[RelationId] = -1, NULL, U.[RelationId]) AS [RelationId1]
		FROM UnCapitalizedExpenses U
		JOIN ActiveRelations AR ON U.[CenterId] = AR.[CenterId] AND U.[RelationId] = AR.[RelationId]
		JOIN TargetResources T
			ON AR.[AccountId] = T.[AccountId]
			AND U.[CenterId] = T.[CenterId] AND U.[RelationId] = T.[RelationId]
		JOIN dbo.Resources R ON R.[Id] = T.[ResourceId]
	) -- select * from ExpenseDistribution
	INSERT INTO @WideLines([Index], [DefinitionId],
			[DocumentIndex], [AccountId0], [CenterId0], [RelationId0], [ResourceId0],[Quantity0],[UnitId0],
			[AccountId1], [CenterId1], [RelationId1], [ResourceId1], [NotedRelationId1], [CurrencyId1],
			[MonetaryValue1], [Value1])
	SELECT	ROW_NUMBER() OVER(ORDER BY ED.[RelationId1], A.[Code], [ResourceId0]) - 1, @LineDefinitionId,
			@DocumentIndex,[AccountId0], [CenterId0], [RelationId0], [ResourceId0], [Quantity0], [UnitId0],
			[AccountId1], [CenterId1], [RelationId1], [ResourceId1], [NotedRelationId1], [CurrencyId1],
			[MonetaryValue1], [Value1]
	FROM ExpenseDistribution ED
	JOIN dbo.Accounts A ON ED.[AccountId1] = A.[Id]
	WHERE [MonetaryValue1] > 0.005 OR [Value1] > 0.005;

	SELECT * FROM @WideLines;