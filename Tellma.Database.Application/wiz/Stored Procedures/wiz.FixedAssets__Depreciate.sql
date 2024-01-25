CREATE PROCEDURE [wiz].[FixedAssets__Depreciate]
	@DocumentIndex	INT = 0,
	@DepreciationPeriodStarts DATE =  N'2023.09.01',
	@DepreciationPeriodEnds DATE =  N'2023.09.30',
	@LineType TINYINT = 100 -- 100: Normal, 120: Regulatory
	-- TODO: Rewrite it so it relies on the table valued function [wiz].[ft_FixedAssets__Depreciate]
	-- or better, replace the call everywhere to this one.
AS
	-- Return the list of assets that have depreciable life, with Time1 = last depreciable date + 1
	-- Time2 is decided by posting date
	DECLARE @WideLines [WidelineList];
	DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
	DECLARE @ROUNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'RightofuseAssets');
	DECLARE @IANode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'IntangibleAssetsOtherThanGoodwill');

	DECLARE @DepreciationAndAmortisationExpenseNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'DepreciationAndAmortisationExpense');
	DECLARE @DepreciatedLife DECIMAL (19, 4) = 1 + DATEDIFF(MONTH, @DepreciationPeriodStarts, @DepreciationPeriodEnds);

	DECLARE @FAAccountIds TABLE ([Id] INT PRIMARY KEY, [EntryTypeId] INT)
	INSERT INTO @FAAccountIds([Id] , [EntryTypeId])
	SELECT A.[Id], 
	CASE
			WHEN AC.[Node].IsDescendantOf(@PPENode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment')
			WHEN AC.[Node].IsDescendantOf(@ROUNode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment')
			WHEN AC.[Node].IsDescendantOf(@IANode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'AmortisationIntangibleAssetsOtherThanGoodwill')
			ELSE NULL
	END AS [EntryTypeId]
	FROM dbo.[Accounts] A
	JOIN dbo.[AccountTypes] AC ON AC.[Id] = A.[AccountTypeId]
	WHERE A.[IsActive] = 1
	AND (
		AC.[Node].IsDescendantOf(@PPENode) = 1 OR
		AC.[Node].IsDescendantOf(@ROUNode) = 1 OR
		AC.[Node].IsDescendantOf(@IANode) = 1
	);
	-- select * from @FAAccountIds

	DECLARE @DNAEAccountIds TABLE ([Id] INT PRIMARY KEY);
	INSERT INTO @DNAEAccountIds
	SELECT [Id] FROM dbo.[Accounts]
	WHERE [IsActive] = 1
	AND [AccountTypeId] IN (
		SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@DepreciationAndAmortisationExpenseNode) = 1);
	-- select * from @DNAEAccountIds

	DECLARE @OpeningBalances TABLE ([ResourceId] INT PRIMARY KEY, [BaseUnitId] INT, [CurrencyId] NCHAR (3), [CenterId] INT, [AgentId] INT, [NotedAgentId] INT,
									[NotedResourceId] INT, [EntryTypeId] INT, [NetLife] DECIMAL (19, 6), [NetMonetaryValue] DECIMAL (19, 6),
									[NetValue] Decimal (19, 6) NOT NULL, [NetResidualMonetaryValue] DECIMAL (19, 6) NOT NULL, [NetResidualValue] DECIMAL (19, 6) NOT NULL);
	INSERT INTO @OpeningBalances([ResourceId], [BaseUnitId], [CurrencyId], [CenterId], [AgentId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
		[NetLife], [NetMonetaryValue], [NetValue], [NetResidualMonetaryValue], [NetResidualValue])
	SELECT E.[ResourceId], E.[BaseUnitId], E.[CurrencyId], E.[CenterId], E.[AgentId], E.[NotedAgentId], E.[NotedResourceId],
		A.EntryTypeId, -- purpose for FA account corresponding to depreciation/amortization
		SUM(E.[Direction] * E.[BaseQuantity]) AS [NetLife],
		SUM(E.[Direction] * E.[MonetaryValue]) AS [NetMonetaryValue],
		SUM(E.[Direction] * E.[Value]) AS [NetValue],
		ISNULL(SUM(E.[Direction] * E.[NotedAmount]), 0) AS [NetResidualMonetaryValue],
		ISNULL(SUM(E.[Direction] * E.[NotedAmount]) * SUM(E.[Direction] * E.[Value]) / SUM(E.[Direction] * E.[MonetaryValue]), 0) AS [NetResidualValue]
	FROM map.DetailsEntries() E
	JOIN dbo.Lines L ON E.LineId = L.Id
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	JOIN @FAAccountIds A ON E.AccountId = A.[Id]
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
	--AND E.Time1 < @DepreciationPeriodStarts - MA, Commented 2023.07.02. Replaced with logic below
	AND E.[ResourceId] IN ( -- for FA which were acquired before period start, we include any additions till period end
		SELECT ResourceId
		FROM Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		WHERE L.[State] = 4
		AND Time1 < @DepreciationPeriodStarts
	)
	AND L.[PostingDate] < @DepreciationPeriodEnds -- Changed from <= to <, to exclude opening balance transactions at end of month
	AND L.[PostingDate] <= @DepreciationPeriodEnds
	AND R.[IsActive] = 1 AND R.[Code] <> N'0'
	GROUP BY E.[ResourceId], E.[BaseUnitId], E.[CurrencyId], E.[CenterId], E.[AgentId], E.[NotedAgentId], E.[NotedResourceId], A.[EntryTypeId]
	HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0;
	-- select * from @OpeningBalances; -- Balance sheet accounts

	DECLARE @LastDepreciationDates TABLE ([ResourceId] INT PRIMARY KEY, [LastDepreciationDate] DATE);
	INSERT INTO @LastDepreciationDates([ResourceId], [LastDepreciationDate])
	SELECT E.[ResourceId], MAX(L.[PostingDate]) AS LastDepreciationDate
	FROM map.DetailsEntries() E
	JOIN dbo.Lines L ON E.LineId = L.Id
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	JOIN @DNAEAccountIds A ON E.AccountId = A.[Id]
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
	AND L.PostingDate < @DepreciationPeriodStarts
	AND R.[IsActive] = 1 AND R.[Code] <> N'0'
	GROUP BY E.[ResourceId];
	-- select * from @LastDepreciationDates
	
	DECLARE @OpeningAgentNotedResourceNotedAgentEntryTypes TABLE ([ResourceId] INT PRIMARY KEY, [AgentId] INT, [NotedResourceId] INT, [NotedAgentId] INT, [EntryTypeId] INT);
	INSERT INTO @OpeningAgentNotedResourceNotedAgentEntryTypes([ResourceId], [AgentId], [NotedResourceId], [NotedAgentId], [EntryTypeId])
	SELECT E.[ResourceId], E.[AgentId], E.[NotedResourceId], E.[NotedAgentId], E.[EntryTypeId]-- to copy the last depreciation purpose, in the case of business unit.
	FROM map.DetailsEntries() E
	JOIN dbo.Lines L ON E.LineId = L.Id
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	JOIN @DNAEAccountIds A ON E.AccountId = A.[Id]
	JOIN @LastDepreciationDates LDD ON LDD.ResourceId = E.[ResourceId] AND LDD.[LastDepreciationDate] = L.[PostingDate]
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
	AND R.[IsActive] = 1 AND R.[Code] <> N'0';
	-- select * from @OpeningAgentNotedResourceNotedAgentEntryTypes; -- P/L accounts
	
	-- As it is now, it is run every month, and if you want to include the items purchased before mid month, simply adjust
	-- Date 1: Purchase Date, Date 2: Available for itended use, From Date: Depreciation Starts, To Date: Depreciation Ends

	DECLARE @PeriodAdditions TABLE ([ResourceId] INT PRIMARY KEY
	, [BaseUnitId] INT, [CurrencyId] NCHAR (3), [CenterId] INT, [AgentId] INT, [NotedAgentId] INT,
									[NotedResourceId] INT, [EntryTypeId] INT,
									[DepreciableLife] DECIMAL (19, 6) NOT NULL, [NetLife] DECIMAL (19, 6) NOT NULL,
									[NetMonetaryValue] DECIMAL (19, 6) NOT NULL, [NetValue] Decimal (19, 6) NOT NULL,
									[NetResidualMonetaryValue] DECIMAL (19, 6) NOT NULL, [NetResidualValue] DECIMAL (19, 6) NOT NULL);
	INSERT INTO @PeriodAdditions([ResourceId], [BaseUnitId], [CurrencyId], [CenterId], [AgentId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
		[DepreciableLife], [NetLife], [NetMonetaryValue], [NetValue], [NetResidualMonetaryValue], [NetResidualValue])
	SELECT E.[ResourceId], E.[BaseUnitId], E.[CurrencyId], E.[CenterId], E.[AgentId], E.[NotedAgentId], E.[NotedResourceId],
	A.[EntryTypeId], -- purpose for FA account corresponding to depreciation/amortization
		(1.0 + DATEDIFF(DAY, MIN(E.[Time1]), @DepreciationPeriodEnds)) /
		(1.0 + DATEDIFF(DAY, @DepreciationPeriodStarts, @DepreciationPeriodEnds)) AS DepreciableLife,
		SUM(E.[Direction] * E.[BaseQuantity]) AS [NetLife],
		SUM(E.[Direction] * E.[MonetaryValue]) AS [NetMonetaryValue],
		SUM(E.[Direction] * E.[Value]) AS [NetValue],
		SUM(E.[Direction] * E.[NotedAmount]) AS [NetResidualMonetaryValue],
		-- When both num and denom are zero, we use zero.
		-- This situation is not normal and only happens when user enters data
		-- in some reverse order. But we added this handling to make the code bullet proof.
		-- Ideally, we should handle wrong data entry first.
		IIF(SUM(E.[Direction] * E.[NotedAmount]) * SUM(E.[Direction] * E.[Value]) = 0, 0, 
			SUM(E.[Direction] * E.[NotedAmount]) * SUM(E.[Direction] * E.[Value])
			/ SUM(E.[Direction] * E.[MonetaryValue]) 
			) AS [NetResidualValue]
	FROM map.DetailsEntries() E
	JOIN dbo.Lines L ON E.LineId = L.Id
	JOIN map.Documents() D ON D.Id = L.DocumentId
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	JOIN @FAAccountIds A ON E.AccountId = A.[Id]
	JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
	-- MA: Added 2024-01-17 line below to exclude FA added by JV as opening balance. Note using < instead of <=
	-- it is necessary since opening balance is last date of month
	AND L.[PostingDate] < @DepreciationPeriodEnds 
	AND E.Time1 Between @DepreciationPeriodStarts AND @DepreciationPeriodEnds
	AND ET.[Concept] IN (
		N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment',--		N'InternalTransferPropertyPlantAndEquipmentExtension',
		N'AdditionsOtherThanThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill' --N'InternalTransferIntangibleAssetsOtherThanGoodwillExtension'
		)
	AND E.[ResourceId] NOT IN (SELECT [ResourceId] FROM @OpeningBalances)
	AND R.[IsActive] = 1 AND R.[Code] <> N'0'
	GROUP BY E.[ResourceId], E.[BaseUnitId], E.[CurrencyId], E.[CenterId], E.[AgentId], E.[NotedAgentId], E.[NotedResourceId], A.[EntryTypeId]
	HAVING SUM(E.[Direction] * E.[BaseQuantity]) <> 0
	OR SUM(E.[Direction] * E.[MonetaryValue]) <> 0;
	-- select * from @PeriodAdditions;

	DECLARE @PeriodDisposals TABLE ([ResourceId] INT PRIMARY KEY)
	INSERT INTO @PeriodDisposals([ResourceId])
	SELECT E.[ResourceId]-- SUM(E.[Direction] * E.[BaseQuantity]), SUM(E.[Direction] * E.[MonetaryValue])
	FROM map.DetailsEntries() E
	JOIN dbo.Lines L ON E.LineId = L.Id
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	JOIN @FAAccountIds A ON E.AccountId = A.[Id]
	WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
	GROUP BY E.[ResourceId]
	HAVING 	ABS(SUM(E.[Direction] * E.[BaseQuantity])) < 0.01 AND ABS(SUM(E.[Direction] * E.[MonetaryValue])) < 0.01
	--select * from @PeriodDisposals

	DECLARE @TargetPeriodDepreciation TABLE ([EntryTypeId] INT, [ResourceId] INT, [BaseUnitId] INT, [CurrencyId] NCHAR (3), [CenterId] INT, [AgentId] INT, [NotedAgentId] INT, [NotedResourceId] INT,
		[NetLife] DECIMAL (19, 6) NOT NULL, [NetMonetaryValue] DECIMAL (19, 6) NOT NULL, [NetValue]  DECIMAL (19, 6) NOT NULL);
	INSERT INTO @TargetPeriodDepreciation([EntryTypeId], [ResourceId], [BaseUnitId], [CurrencyId], [CenterId], [AgentId], [NotedAgentId], [NotedResourceId], [NetLife], [NetMonetaryValue], [NetValue])
	SELECT OB.[EntryTypeId], OB.[ResourceId], OB.[BaseUnitId], OB.[CurrencyId], OB.[CenterId], OB.[AgentId], OB.[NotedAgentId], OB.[NotedResourceId],
		IIF(OB.[NetLife] < @DepreciatedLife, OB.[NetLife], @DepreciatedLife) AS [NetLife], --<< need review
		@DepreciatedLife * bll.fn_BookValue_Residual_LifeTime__Depreciation(ISNULL(LK.[Code], N'SL'),
			OB.[NetMonetaryValue] - OB.[NetResidualMonetaryValue], OB.[NetResidualMonetaryValue], OB.[NetLife]
		) AS [NetMonetaryValue],
		@DepreciatedLife * bll.fn_BookValue_Residual_LifeTime__Depreciation(ISNULL(LK.[Code], N'SL'),
			OB.[NetValue] - OB.[NetResidualValue], OB.[NetResidualValue], OB.[NetLife]
		) AS [NetValue]
	FROM @OpeningBalances OB
	JOIN dbo.Resources PPE ON PPE.[Id] = OB.[ResourceId]
	LEFT JOIN dbo.Lookups LK ON LK.Id = PPE.[Lookup4Id] -- assuming we use last lookup for dep methods
	WHERE PPE.[IsActive] = 1 AND PPE.[Code] <> N'0'
	UNION
	SELECT PA.[EntryTypeId], PA.[ResourceId], PA.[BaseUnitId], PA.[CurrencyId], PA.[CenterId], PA.[AgentId], PA.[NotedAgentId], PA.[NotedResourceId],	
		PA.[DepreciableLife] AS [NetLife],
		PA.[DepreciableLife] * bll.fn_BookValue_Residual_LifeTime__Depreciation(ISNULL(LK.[Code], N'SL'),
			PA.[NetMonetaryValue] - PA.[NetResidualMonetaryValue], PA.[NetResidualMonetaryValue], PA.[NetLife]
		) AS [NetMonetaryValue],
		PA.[DepreciableLife] * bll.fn_BookValue_Residual_LifeTime__Depreciation(ISNULL(LK.[Code], N'SL'),
			PA.[NetValue] - PA.[NetResidualValue], PA.[NetResidualValue], PA.[NetLife]
		) AS [NetValue]
	FROM @PeriodAdditions PA
	JOIN dbo.Resources PPE ON PPE.[Id] = PA.[ResourceId]
	LEFT JOIN dbo.Lookups LK ON LK.Id = PPE.[Lookup4Id] -- assuming we use last lookup for dep methods
	WHERE PPE.[IsActive] = 1 AND PPE.[Code] <> N'0'
	AND PPE.[Id] NOT IN (SELECT [ResourceId] FROM @PeriodDisposals)
	-- select * from @TargetPeriodDepreciation; -- balance sheet accounts

	-- Note: To handle the case of FA changing multiple centers during the month, we can make an ajusting entries for those who changed only
	INSERT INTO @WideLines([Index],
		[DocumentIndex], 
		[CenterId0], [CurrencyId0], [AgentId0], [ResourceId0], [NotedAgentId0], [NotedResourceId0], [Quantity0], [UnitId0], [MonetaryValue0], [Value0], [Time10], [Time20],[EntryTypeId0],
		[CenterId1], [CurrencyId1], [AgentId1], [ResourceId1], [NotedAgentId1], [NotedResourceId1], [Quantity1], [UnitId1], [MonetaryValue1], [Value1], [Time11], [Time21],[EntryTypeId1]
		)
	SELECT ROW_NUMBER() OVER(ORDER BY NPD.[ResourceId]) - 1,
			@DocumentIndex,
			NPD.[CenterId], NPD.[CurrencyId], NPD.[AgentId], NPD.[ResourceId],	NPD.[NotedAgentId], OANRNAET.[NotedResourceId],
			[NetLife], R.[UnitId], [NetMonetaryValue], [NetValue], @DepreciationPeriodStarts, @DepreciationPeriodEnds, OANRNAET.[EntryTypeId],
			NPD.[CenterId], NPD.[CurrencyId], NPD.[AgentId], NPD.[ResourceId], NPD.[NotedAgentId], NPD.[NotedResourceId],
			[NetLife], R.[UnitId], [NetMonetaryValue], [NetValue], @DepreciationPeriodStarts, @DepreciationPeriodEnds, NPD.[EntryTypeId]
	FROM @TargetPeriodDepreciation NPD
	JOIN dbo.Resources R ON R.[Id] = NPD.[ResourceId]
	LEFT JOIN @OpeningAgentNotedResourceNotedAgentEntryTypes OANRNAET ON OANRNAET.[ResourceId] = NPD.[ResourceId];

	SELECT * FROM @WideLines;
