CREATE PROCEDURE [bll].[ExpensesByNature__Capitalize]
/* 4: Import, 6: Manufacturing, 74: Adama, 75: AA
	[bll].[ExpensesByNature__Capitalize] 
*/
	@DocumentIndex	INT = 0,
	@NativeAgentDefinitionCode NVARCHAR (255),
	@FromDate DATE = NULL -- if left empty, it takes beginning of open period
AS
-- Specifc to the new version
	IF dal.fn_FeatureCode__IsEnabled(N'BusinessUnitGoneWithTheWind') = 0 RETURN

	DECLARE @ArchiveDate DATE = (SELECT TOP 1 [ArchiveDate] FROM dbo.Settings);
	SET @FromDate = ISNULL(@FromDate, DATEADD(DAY, 1, @ArchiveDate));

	DECLARE	@BSAccountTypeConcept NVARCHAR (255) = 
		CASE
			WHEN @NativeAgentDefinitionCode IN (N'IncomingShipment') THEN N'CurrentInventoriesInTransit'
			WHEN @NativeAgentDefinitionCode IN (N'ProductionOrder') THEN N'WorkInProgress'
			WHEN @NativeAgentDefinitionCode IN (N'ConstructionInProgressMember') THEN N'ConstructionInProgress'
			WHEN @NativeAgentDefinitionCode IN (N'InvestmentPropertyUnderConstructionOrDevelopmentMember') THEN N'InvestmentPropertyUnderConstructionOrDevelopment'
		END

	DECLARE @BSAccountTypeId INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = @BSAccountTypeConcept);
	DECLARE @EntryTypeConcept NVARCHAR (255) = 
		CASE
			WHEN @BSAccountTypeConcept = N'CurrentInventoriesInTransit' THEN N'AdditionsFromPurchasesInventoriesExtension'
			WHEN @BSAccountTypeConcept = N'WorkInProgress' THEN N'CurrentRawMaterialsAndCurrentProductionSuppliesToWorkInProgressInventoriesExtension'
			WHEN @BSAccountTypeConcept = N'ConstructionInProgress' THEN N'???'
			WHEN @BSAccountTypeConcept = N'InvestmentPropertyUnderConstructionOrDevelopment' THEN N'???'
		END
	DECLARE @EntryTypeId INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = @EntryTypeConcept)
	DECLARE @NativeAgentDefinitionId INT = (SELECT [Id] FROM dbo.AgentDefinitions WHERE [Code] = @NativeAgentDefinitionCode);
	DECLARE @OverheadAgentId INT =  dal.fn_AgentDefinition_Code__Id(@NativeAgentDefinitionCode, N'0');

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
		SELECT MIN(E.[Id]) AS [Id], E.[CenterId],
				E.[AgentId],
				E.[CurrencyId], SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue],
				SUM(E.[Direction] * E.[Value])  AS [Value]
		FROM dbo.Entries E
		JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Centers C ON E.[CenterId] = C.[Id]
		WHERE L.[State] = 4
		AND E.[AccountId] IN (SELECT [Id] FROM ExpenseByNatureAccounts)
		AND L.PostingDate >= @FromDate
		AND AG.[DefinitionId] = @NativeAgentDefinitionId
		GROUP BY E.[CenterId],E.[AgentId], E.[CurrencyId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	),--	select ag.Name, UC.* from UnCapitalizedExpenses UC join dbo.Agents ag on ag.id = uc.AgentId
	TargetInput AS (
		SELECT IIF(L.[PostingDate] < @FromDate, @FromDate, L.[PostingDate]) AS [PostingDate],
			E.[AccountId], E.[AgentId],
			E.[ResourceId], SUM(E.[Direction] * E.[BaseQuantity]) AS NetQuantity, SUM(E.[Direction] * E.[Value]) AS NetValue
		FROM map.DetailsEntries() E
		JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN UnCapitalizedExpenses UE ON E.[AgentId] = UE.[AgentId]
		WHERE L.[State] = 4
		AND A.[AccountTypeId] = @BSAccountTypeId
		AND E.EntryTypeId = @EntryTypeId
		GROUP BY L.[PostingDate], E.[AccountId], E.[AgentId], E.[ResourceId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	), --	select ag.Name as agent, TI.* from TargetInput ti join dbo.Accounts a on a.Id = TI.AccountId join dbo.Agents ag on ag.Id = ti.AgentId order by ti.postingdate
	TotalInput AS (
		SELECT [AccountId], [AgentId], -- [ResourceId],
			SUM([NetQuantity]) AS [TotalQuantity], SUM([NetValue]) AS TotalValue
		FROM TargetInput
		GROUP BY [AccountId], [AgentId]--,[ResourceId]
	),
	ExpenseDistribution AS (
		SELECT TI.PostingDate,
			TT.AccountId AS [AccountId0], U.[CenterId] AS [CenterId0], U.[AgentId] AS [AgentId0],
			TI.[ResourceId] AS [ResourceId0], 0 AS [Quantity0], R.[UnitId] AS [UnitId0], 
			U.[CenterId] AS [CenterId1],  U.[AgentId] AS [AgentId1],
			U.[CurrencyId] AS [CurrencyId1],
			-- Assuming the expenses is distributed proportional to the items values. If based on other properties, we need to change it here
			ROUND(U.[MonetaryValue] * TI.[NetValue] / TT.[TotalValue], 2) AS [MonetaryValue1],
			ROUND(U.[Value] * TI.[NetValue] / TT.[TotalValue], 2) AS [Value1]
		FROM UnCapitalizedExpenses U
		JOIN TotalInput TT ON U.[AgentId] = TT.[AgentId]
		JOIN TargetInput TI ON TT.[AccountId] = TI.[AccountId] AND TT.[AgentId] = TI.[AgentId] -- AND TT.[ResourceId] = TI.[ResourceId]
		JOIN dbo.Resources R ON R.[Id] = TI.[ResourceId]
	) -- select ag.name as agent, r.name as resource, ed.* from ExpenseDistribution ed join agents ag on ag.Id = ed.AgentId0 join dbo.resources r on r.Id = ed.ResourceId0 order by ed.PostingDate
	INSERT INTO @WideLines([Index], [DocumentIndex],PostingDate,
			[AccountId0], [CenterId0], [AgentId0], [ResourceId0],[Quantity0],[UnitId0],
			[CenterId1], [AgentId1], [CurrencyId1],
			[MonetaryValue1], [Value1])
	SELECT	ROW_NUMBER() OVER(ORDER BY ED.[AgentId1], [ResourceId0]) - 1,	@DocumentIndex,[Postingdate],
			[AccountId0], [CenterId0], [AgentId0], [ResourceId0], [Quantity0], [UnitId0],
			-- AccountId1 is concluded from AccountType: O/H Allocation, and using the Agent Definition
			[CenterId1], [AgentId1], [CurrencyId1],
			[MonetaryValue1], [Value1]
	FROM ExpenseDistribution ED
	WHERE [MonetaryValue1] > 0.005 OR [Value1] > 0.005;

	SELECT * FROM @WideLines;