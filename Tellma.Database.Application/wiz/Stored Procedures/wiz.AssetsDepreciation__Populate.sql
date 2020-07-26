CREATE PROCEDURE [dbo].[AssetsDepreciation__Populate]
	@DocumentIndex	INT = 0,
	@PostingDate	DATE = N'2020.01.31',
	@Quantity		DECIMAL (19,4) = 1, -- Used Capacity
	@UnitId			INT = 8, -- select * from Units
	@Time1			DATETIME2 (2) = N'2020.01.01',
	@Time2			DATETIME2 (2) = N'2020.01.31'
AS
	DECLARE @LineDefinitionId INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEDepreciation');
	-- Raise Exception if some information is missing

--(0,8,	N'Entries', N'ResourceId',			1,	N'Asset',		N'الأصل',		1,4,0),
--(1,8,	N'Entries', N'Quantity',			1,	N'Usage',		N'الاستخدام',	1,4,1),
--(2,8,	N'Entries', N'UnitId',				1,	N'',			N'',			1,4,1),
--(3,8,	N'Entries', N'CenterId',			0,	N'Cost Ctr',	N'مركز التكلفة',1,4,0),
--(4,8,	N'Entries', N'EntryTypeId',			0,	N'Purpose',		N'الغرض',		1,4,0),
--(5,8,	N'Entries', N'Time1',				1,	N'From',		N'ابتداء من',	1,4,1),
--(6,8,	N'Entries', N'Time2',				1,	N'Till',		N'حتى',			1,0,1),
--(7,8,	N'Entries', N'MonetaryValue',		1,	N'Depreciation',N'الإهلاك',		1,0,0);
	DECLARE @WideLines WideLineList;
	DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
	DECLARE @PPETypeIds IdList;
	DECLARE @AET INT = (
		SELECT [Id] FROM dbo.EntryTypes
		WHERE [Concept] = N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment'
	);

	WITH PPEBalancesPre AS (
		SELECT
				--SUM(E.[Direction] * IIF(E.[EntryTypeId] = @AET, E.[Quantity], 0)) AS TotalCapacity,
				--SUM(E.[Direction] * IIF(E.[EntryTypeId] = @AET, E.[MonetaryValue], 0)) AS TotalMonetaryValue,
				--SUM(E.[Direction] * IIF(E.[EntryTypeId] = @AET, E.[Value], 0)) AS TotalValue,

				SUM(E.[Direction] * E.[Quantity]) AS RemainingCapacity,
				SUM(E.[Direction] * E.[MonetaryValue]) AS RemainingMonetaryValue,
				SUM(E.[Direction] * E.[Value]) AS RemainingValue,
				E.[ResourceId]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.Accounts A ON E.AccountId = A.[Id]
		JOIN dbo.Units U ON E.[UnitId] = U.[Id]
		WHERE A.[AccountTypeId] IN (SELECT [Id] FROM @PPETypeIds)
		AND L.[State] = 4
		AND L.[PostingDate] <= @PostingDate
		AND U.[UnitType] <> N'Pure'
		GROUP BY E.[ResourceId]
		HAVING SUM(E.[Direction] * E.[Quantity]) > 0 OR SUM(E.[Direction] * E.[MonetaryValue]) > 0 OR  SUM(E.[Direction] * E.[Value]) > 0
	),
	PPEBalances AS (
	-- Total information is used for accelerated depreciation models, when we implement them
		SELECT --TotalCapacity, TotalMonetaryValue, TotalValue,
				PB.RemainingCapacity,
				PB.RemainingMonetaryValue AS [DepreciableRemainingMonetaryValue],
				PB.RemainingValue AS [DepreciableRemainingValue],
				IIF(@Quantity <  PB.RemainingCapacity, @Quantity,  PB.RemainingCapacity) AS [UsedCapacity],
				PB.[ResourceId]
		FROM PPEBalancesPre PB
		JOIN dbo.[Resources] R ON PB.ResourceId = R.Id
		WHERE RemainingCapacity > 0
	)
	-- Linear Depreciation Model, and units of production model
	INSERT INTO @WideLines([Index], DefinitionId,
			[DocumentIndex],ResourceId1,
			Quantity1,
			[MonetaryValue1],
			[Value1],
			UnitId1, Time11,	Time21,
			CurrencyId1,	CenterId0)
	SELECT	ROW_NUMBER() OVER(ORDER BY [Id]) - 1, @LineDefinitionId,
			@DocumentIndex, R.[Id],
			PB.[UsedCapacity],
			PB.[DepreciableRemainingMonetaryValue] * PB.[UsedCapacity] / PB.[RemainingCapacity],
			PB.[DepreciableRemainingValue] * PB.[UsedCapacity] / PB.[RemainingCapacity],
			@UnitId, @Time1,	@Time2, 
			[CurrencyId],	[CenterId]
	FROM dbo.[Resources] R
	JOIN PPEBalances PB ON R.[Id] = PB.[ResourceId];

	SELECT [Index], [DocumentIndex], ResourceId1 AS [Asset], Quantity1 AS [UsedCapacity],
			(SELECT [Name] FROM dbo.[Units] WHERE [Id] = WL.UnitId1) AS Unit,
			MonetaryValue1 As UsedMonetaryValue,
			Value1 As UsedValue, 
			CustodianId0 AS [System],
			--(SELECT [Name] FROM dbo.EntryTypes WHERE [Id] = EntryTypeId0) AS Purpose,
			Time11 AS FromDate,	Time21 AS ToDate,
			CurrencyId1 AS Currency, CenterId0 AS ExpCenter
	FROM @WideLines WL;