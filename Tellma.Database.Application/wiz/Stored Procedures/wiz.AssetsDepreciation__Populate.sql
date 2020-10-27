CREATE PROCEDURE [wiz].[AssetsDepreciation__Populate]
	@DocumentIndex	INT = 0,
	@PostingDate	DATE = N'2020.07.31',
	@Quantity		DECIMAL (19,4) = NULL, -- Used Capacity
	@UnitId			INT = NULL,
	@Time1			DATETIME2 (2) = NULL,
	@Time2			DATETIME2 (2) = NULL
AS
	DECLARE @LineDefinitionId INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEDepreciation');
	
	IF @UnitId IS NULL
		SELECT @UnitId = [Id] FROM dbo.Units WHERE [Code] = N'mo';
	
	SELECT @Quantity = ISNULL(@Quantity, 1), @Time2 = ISNULL(@Time2, @PostingDate)
	
	IF @Time1 IS NULL
	AND (SELECT [Code] FROM dbo.Units WHERE [Id] = @UnitId) = N'mo'
		SET @Time1 = DATEADD(MONTH, -@Quantity, DATEADD(DAY,+1,@Time2));

	DECLARE @WideLines WideLineList;
	DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
	DECLARE @PPEDepreciationExpenseNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'DepreciationExpense');
	
	DECLARE @PPETypeIds IdList;
	DECLARE @AET INT = (
		SELECT [Id] FROM dbo.EntryTypes
		WHERE [Concept] = N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment'
	);

	WITH PPEAccountIds AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [AccountTypeId] IN (
			SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@PPENode) = 1
		)
	),
	PPEDepreciationExpenseAccountIds AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [AccountTypeId] IN (
			SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@PPENode) = 1
		)
	),
	PPEBalancesPre AS (
		SELECT
				--SUM(E.[Direction] * IIF(E.[EntryTypeId] = @AET, E.[Quantity], 0)) AS TotalCapacity,
				--SUM(E.[Direction] * IIF(E.[EntryTypeId] = @AET, E.[MonetaryValue], 0)) AS TotalMonetaryValue,
				--SUM(E.[Direction] * IIF(E.[EntryTypeId] = @AET, E.[Value], 0)) AS TotalValue,

				SUM(E.[Direction] * E.[Quantity]) AS RemainingCapacity,
				SUM(E.[Direction] * E.[MonetaryValue]) AS RemainingMonetaryValue,
				SUM(E.[Direction] * E.[Value]) AS RemainingValue,
				E.[ResourceId],
				E.[CustodyId],
				E.[CenterId]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		JOIN dbo.Units U ON E.[UnitId] = U.[Id]
		WHERE L.[State] = 4
		AND L.[PostingDate] <= @PostingDate
		AND E.UnitId = @UnitId
		GROUP BY E.[ResourceId], E.[CustodyId], E.[CenterId]
		HAVING SUM(E.[Direction] * E.[Quantity]) > 0 OR SUM(E.[Direction] * E.[MonetaryValue]) > 0 OR  SUM(E.[Direction] * E.[Value]) > 0
	),
	PPEBalances AS (
	-- Total information is used for accelerated depreciation models, when we implement them
		SELECT --TotalCapacity, TotalMonetaryValue, TotalValue,
				PB.RemainingCapacity,
				PB.RemainingMonetaryValue AS [DepreciableRemainingMonetaryValue],
				PB.RemainingValue AS [DepreciableRemainingValue],
				IIF(@Quantity <  PB.RemainingCapacity, @Quantity,  PB.RemainingCapacity) AS [UsedCapacity],
				PB.[ResourceId], PB.[CustodyId], PB.[CenterId]
		FROM PPEBalancesPre PB
		JOIN dbo.[Resources] R ON PB.ResourceId = R.Id
		WHERE RemainingCapacity > 0
	),
	LastPPEEntries AS (
		SELECT MAX(E.[Id]) AS LastEntryId, E.ResourceId
		FROM dbo.Lines L
		JOIN dbo.Entries E ON L.[Id] = E.[LineId]
		JOIN PPEDepreciationExpenseAccountIds A ON E.[AccountId] = A.[Id]
		WHERE L.[State] = 4
		GROUP BY E.ResourceId
	),
	LastCostCenters AS (
		SELECT E.ResourceId, E.CenterId
		FROM dbo.Entries E
		JOIN LastPPEEntries LE ON E.[Id] = LE.LastEntryId
	)
	-- Linear Depreciation Model, and units of production model
	INSERT INTO @WideLines([Index], [DefinitionId],
			[PostingDate],
			[DocumentIndex],[ResourceId1],[CustodyId1],[CenterId1],[CenterId0],
			[Quantity1],
			[MonetaryValue1],
			[Value1],
			[UnitId1], [Time10], [Time20],
			[CurrencyId0], [CurrencyId1])
	SELECT	ROW_NUMBER() OVER(ORDER BY [Id]) - 1, @LineDefinitionId,
			@PostingDate,
			@DocumentIndex, R.[Id], PB.CustodyId, PB.CenterId, LCC.CenterId,
			PB.[UsedCapacity],
			PB.[DepreciableRemainingMonetaryValue] * PB.[UsedCapacity] / PB.[RemainingCapacity],
			PB.[DepreciableRemainingValue] * PB.[UsedCapacity] / PB.[RemainingCapacity],
			@UnitId, @Time1,	@Time2, 
			R.[CurrencyId], R.[CurrencyId]
	FROM dbo.[Resources] R
	JOIN PPEBalances PB ON R.[Id] = PB.[ResourceId]
	LEFT JOIN LastCostCenters LCC ON R.[Id] = LCC.[ResourceId]

	SELECT * FROM @WideLines;