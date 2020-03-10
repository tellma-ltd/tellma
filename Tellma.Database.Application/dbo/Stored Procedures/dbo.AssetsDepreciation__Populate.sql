CREATE PROCEDURE [dbo].[AssetsDepreciation__Populate]
	@DocumentIndex	INT = 0,
	@DocumentDate	DATE = N'2020.01.31',
	@UsedCapacity	DECIMAL (19,4) = 1,
	@UnitId			INT = 8, -- select * from measurementunits
	@Time1			DATETIME2 (2) = N'2020.01.01',
	@Time2			DATETIME2 (2) = N'2020.01.31',
	@InvestmentCenterId	INT = 1
AS
	DECLARE @LineDefinitionId NVARCHAR (50) = N'PPEDepreciation';
	-- Raise Exception if some information is missing

--(0,8,	N'Entries', N'ResourceId',			1,	N'Asset',		N'الأصل',		1,4,0),
--(1,8,	N'Entries', N'Quantity',			1,	N'UsedCapacity',		N'الاستخدام',	1,4,1),
--(2,8,	N'Entries', N'UnitId',				1,	N'',			N'',			1,4,1),
--(3,8,	N'Entries', N'AgentId',				0,	N'For',			N'لصالح',		1,4,0),
--(4,8,	N'Entries', N'EntryTypeId',			0,	N'Purpose',		N'الغرض',		1,4,0),
--(5,8,	N'Entries', N'Time1',				1,	N'From',		N'ابتداء من',	1,4,1),
--(6,8,	N'Entries', N'Time2',				1,	N'Till',		N'ابتداء من',	1,0,1),
--(7,8,	N'Entries', N'CurrencyId',			1,	N'Currency',	N'العملة',		1,0,0),
--(8,8,	N'Entries', N'MonetaryValue',		1,	N'Depreciation',N'الإهلاك',		1,0,0),
--(9,8,	N'Entries', N'Value',				1,	N'Equiv. ($)',	N'المقابل ($)',	1,4,0),
--(10,8,	N'Entries',	N'ResponsibilityCenterId',1,N'Inv. Ctr',	N'مركز الاستثمار',4,4,1),
--(11,8,	N'Entries',	N'ResponsibilityCenterId',0,N'Cost Ctr',	N'مركز التكلفة',4,4,0);
	DECLARE @WideLines WideLineList;
	DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Code] = N'');
	DECLARE @PPETypeIds IdList;
	DECLARE @AET INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment')

	INSERT INTO @PPETypeIds([Id])
	SELECT [Id] FROM dbo.AccountTypes
	WHERE IsResourceClassification = 1
	AND [Node].IsDescendantOf(@PPENode) = 1; -- select * from entrytypes select * from responsibilitycenters

	INSERT INTO @WideLines([Index], DefinitionId,
			[DocumentIndex],ResourceId1,Quantity1,		UnitId1,AgentId0,			EntryTypeId0,				Time11,	Time21,
			CurrencyId1,	ResponsibilityCenterId1,ResponsibilityCenterId0)
	SELECT	ROW_NUMBER() OVER(ORDER BY [Id]) - 1, @LineDefinitionId,
			@DocumentIndex, [Id],		@UsedCapacity,	@UnitId, 4 AS 'CostObjectId',	16 AS 'ExpenseEntryTypeId',	@Time1,	@Time2, 
			[CurrencyId],	@InvestmentCenterId,	1 AS 'ExpenseResponsibilityCenterId'
	FROM dbo.Resources
	WHERE AccountTypeId IN (SELECT [Id] FROM @PPETypeIds);

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
	JOIN dbo.Documents D ON L.DocumentId = E.Id
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	WHERE A.AccountTypeId IN (SELECT [Id] FROM @PPETypeIds)
	AND L.[State] = 4 AND D.PostingState = 1
	AND D.DocumentDate <= @DocumentDate
	GROUP BY E.[ResourceId]
	HAVING SUM(E.[Direction] * E.[Quantity]) <> 0 OR SUM(E.[Direction] * E.[MonetaryValue]) <> 0 OR  SUM(E.[Direction] * E.[Value]) <> 0
	),
	PPEBalances AS (
	-- Total information is used for accelerated depreciation models, when we implement them
		SELECT --TotalCapacity, TotalMonetaryValue, TotalValue,
				PB.RemainingCapacity,
				-- To handle residual values, use this instead
				--(PB.RemainingMonetaryValue - R.ResidualMonetaryValue) AS [DepreciableRemainingMonetaryValue],
				(PB.RemainingMonetaryValue - 0) AS [DepreciableRemainingMonetaryValue],
				-- To handle residual values, use this instead
				--(PB.RemainingValue - R.ResidualValue) AS [DepreciableRemainingVaue],
				(PB.RemainingValue - 0) AS [DepreciableRemainingValue],
				IIF(@UsedCapacity <  PB.RemainingCapacity, @UsedCapacity,  PB.RemainingCapacity) AS [UsedCapacity],
				PB.[ResourceId]
		FROM PPEBalancesPre PB
		JOIN dbo.Resources R ON PB.ResourceId = R.Id
		WHERE RemainingCapacity > 0
	)
	-- Linear Depreciation Model, and units of production model
	UPDATE WL
	SET
		[Quantity] = PB.[UsedCapacity],
		[MonetaryValue] = PB.[DepreciableRemainingMonetaryValue] * PB.[UsedCapacity] / PB.[RemainingCapacity],
		[Value] = PB.[DepreciableRemainingValue] * PB.[UsedCapacity] / PB.[RemainingCapacity]
	FROM @WideLines WL
	JOIN PPEBalances PB ON WL.ResourceId1 = PB.ResourceId

	SELECT [Index], [DocumentIndex], ResourceId1 AS [Asset], Quantity1 AS [UsedCapacity],
			(SELECT [Name] FROM dbo.MeasurementUnits WHERE [Id] = WL.UnitId1) AS Unit, MonetaryValue1 As UsedMonetaryValue, Value1 As UsedValue, 
			AgentId0 AS [System],
			(select [Name] FROM dbo.EntryTypes WHERE [Id] = EntryTypeId0) AS Purpose,
			Time11 AS FromDate,	Time21 AS ToDate,
			CurrencyId1 AS Currency, ResponsibilityCenterId1 AS InvCtr,ResponsibilityCenterId0 AS PLCenter
	FROM @WideLines WL;