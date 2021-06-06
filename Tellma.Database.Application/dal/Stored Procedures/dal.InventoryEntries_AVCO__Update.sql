CREATE PROCEDURE [dal].[InventoryEntries_AVCO__Update] -- [dal].[InventoryEntries_AVCO__Update] 0
@VerifyLineDefinitions BIT = 0 -- Bug with value = 1
AS
	DECLARE @AffectedLineDefinitionEntries TABLE (
		[LineDefinitionId] INT,
		[Index] INT
		PRIMARY KEY ([LineDefinitionId], [Index])
	);

	DECLARE @T TABLE (
		[Id]					INT PRIMARY KEY IDENTITY,	
		[AccountId]				INT,
		[CenterId]				INT,
		[CustodyId]				INT,
		[ResourceId]			INT,
		[PostingDate]			DATE,
		[Direction]				SMALLINT,
		[AlgebraicQuantity]		DECIMAL (19, 4),
		[AlgebraicMonetaryValue]DECIMAL (19, 4),
		[AlgebraicValue]		DECIMAL (19, 4),
		[RunningQuantity]		DECIMAL (19, 4),
		[RunningMonetaryValue]	DECIMAL (19, 4),
		[RunningValue]			DECIMAL (19, 4),
		[PriorMVPU]				FLOAT (53) DEFAULT (0),
		[PriorVPU]				FLOAT (53) DEFAULT (0)
	);

	DECLARE @BadLineDefinitionId INT;
	DECLARE @ManualLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
	SET NOCOUNT ON;
	
	-- Focus on inventory accounts whose value - when credited - is calculated using AVCO
	WITH InventoryAccountTypes AS (
		SELECT ATC.[Id]
		FROM dbo.AccountTypes ATC
		JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node])  = 1
		WHERE ATP.[Concept] = N'Inventories'
		-- For the WIP concept, whether job or process costing, the credit value is not calculated using AVCO.
		-- For process, we "absorb" from O/H upon declaring production
		AND ATC.Concept NOT IN (N'WorkInProgress', N'CurrentInventoriesInTransit')
	)
	-- Look for smart entries where one of the above inventory accounts appears on the credit side
	INSERT INTO @AffectedLineDefinitionEntries([LineDefinitionId], [Index])
	SELECT [LineDefinitionId], [Index]
	FROM dbo.LineDefinitionEntries
	WHERE [ParentAccountTypeId] IN (SELECT [Id] FROM InventoryAccountTypes)
	AND [Direction] = -1;
	-- Assume the debit entry comes before it (we test the assumption below), and that the values are balanced
	INSERT INTO @AffectedLineDefinitionEntries([LineDefinitionId], [Index])
	SELECT [LineDefinitionId], [Index] - 1
	FROM @AffectedLineDefinitionEntries;
	-- Check if posted entries are balanced already
	IF @VerifyLineDefinitions = 1
		SELECT @BadLineDefinitionId = LD.[LineDefinitionId]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
		JOIN @AffectedLineDefinitionEntries LD ON LD.LineDefinitionId = L.[DefinitionId] AND LD.[Index] = E.[Index]
		WHERE L.[State] = 4
		AND DD.[DocumentType] = 2
		GROUP BY LD.[LineDefinitionId]
		HAVING SUM(E.[Direction] * E.[Value]) <> 0
	-- if not, then the assumption is wrong. Exit.
	IF	@BadLineDefinitionId IS NOT NULL
	BEGIN
		DECLARE @BadLineDefinition NVARCHAR (255);
		SELECT @BadLineDefinition = N'Improper Line Definition Design: ' + [TitleSingular] + N'. The debit should come before the credit for inventory issue.'
		FROM dbo.LineDefinitions
		WHERE [Id] = @BadLineDefinitionId;

		RAISERROR(@BadLineDefinition, 16, 1)
		RETURN
	END;
	-- Initial conditions: look for inventory entries
	WITH InventoryAccountTypes AS (
		SELECT ATC.[Id]
		FROM dbo.AccountTypes ATC
		JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node])  = 1
		WHERE ATP.[Concept] = N'Inventories'
		-- For the WIP concept, whether job or process costing, the credit value is not calculated using AVCO.
		-- For process, we "absorb" from O/H upon declaring production
		AND ATC.Concept NOT IN (N'WorkInProgress', N'CurrentInventoriesInTransit')
	),
	InventoryAccounts AS (
		SELECT A.[Id]
		FROM dbo.Accounts A
		WHERE AccountTypeId IN (SELECT [Id] FROM InventoryAccountTypes)
	),
	AccummulatedEntries AS (
		SELECT  E.[AccountId], E.[CenterId], E.[CustodyId], E.[ResourceId], L.[PostingDate], E.[Direction], 
			SUM(E.[Direction] * E.[BaseQuantity]) AS [AlgebraicQuantity],
			SUM(E.[Direction] * E.[MonetaryValue]) AS [AlgebraicMonetaryValue],
			SUM(E.[Direction] * E.[Value]) AS [AlgebraicValue]
		FROM map.DetailsEntries() E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
		WHERE E.[AccountId] IN (SELECT [Id] FROM InventoryAccounts)
		AND L.[State] = 4
		AND DD.[DocumentType] = 2
		GROUP BY E.[AccountId], E.[CenterId], E.[CustodyId], E.[ResourceId], L.[PostingDate], E.[Direction]
	)
	INSERT INTO @T([AccountId], [CenterId], [CustodyId], [ResourceId], [PostingDate], [Direction], 
		[AlgebraicQuantity], [AlgebraicMonetaryValue], [AlgebraicValue],
		[RunningQuantity], [RunningMonetaryValue], [RunningValue])
	SELECT [AccountId], [CenterId], [CustodyId], [ResourceId], [PostingDate], [Direction],
		[AlgebraicQuantity], [AlgebraicMonetaryValue], [AlgebraicValue],
		SUM([AlgebraicQuantity]) OVER (PARTITION BY [AccountId], [CustodyId], [ResourceId] ORDER BY [PostingDate], [Direction] DESC) AS RunningQuantity,
		SUM([AlgebraicMonetaryValue]) OVER (PARTITION BY [AccountId], [CustodyId], [ResourceId] ORDER BY [PostingDate], [Direction] DESC) AS RunningMonetaryValue,
		SUM([AlgebraicValue]) OVER (PARTITION BY [AccountId], [CustodyId], [ResourceId] ORDER BY [PostingDate], [Direction] DESC) AS RunningValue
	FROM AccummulatedEntries
	ORDER BY [AccountId], [CenterId], [CustodyId], [ResourceId], [PostingDate], [Direction] DESC;
	
	DECLARE @RowCount INT = -1, @PrevRowCount INT = -1, @LoopCounter INT = 0;
	WHILE (1 = 1)
	BEGIN -- Loop to calculate AVCO
		SET @PrevRowCount = @RowCount; SET @LoopCounter = @LoopCounter + 1;
		UPDATE @T
		SET
			PriorMVPU = IIF(
				[RunningQuantity] = [AlgebraicQuantity],
				0,
				([RunningMonetaryValue] - [AlgebraicMonetaryValue]) / ([RunningQuantity] - [AlgebraicQuantity])),
			PriorVPU =  IIF(
				[RunningQuantity] = [AlgebraicQuantity],
				0,
				([RunningValue] - [AlgebraicValue]) / ([RunningQuantity] - [AlgebraicQuantity]));
		-- Look for earliest smart issue dates for each item in each inventory
		WITH BatchStartAndVPU AS (
			SELECT T.[AccountId], T.[CenterId], T.[CustodyId], T.[ResourceId], MIN(T.[PostingDate]) As [PostingDate]
			FROM @T T
			WHERE [Direction] = -1
			AND (T.[AlgebraicMonetaryValue] / T.[AlgebraicQuantity] <> T.[PriorMVPU]
				OR	T.[AlgebraicValue] / T.[AlgebraicQuantity] <> T.[PriorVPU])
			GROUP BY T.[AccountId], T.[CenterId], T.[CustodyId], T.[ResourceId]
		),
		-- Look for first date (smart or JV) where the CPU has deviated from Prior CPU
		BatchEnd AS (
			SELECT T.[AccountId], T.[CenterId], T.[CustodyId], T.[ResourceId], MIN(T.[PostingDate]) As [PostingDate]
			FROM @T T
			JOIN BatchStartAndVPU BS ON T.[AccountId] = BS.[AccountId] AND T.[CenterId] = BS.[CenterId]
				AND T.[CustodyId] = BS.[CustodyId] AND T.[ResourceId] = BS.[ResourceId]
			WHERE [Direction] = 1
			AND T.[AlgebraicQuantity] <> 0
			AND T.[PostingDate] > BS.[PostingDate]
			AND (T.[AlgebraicMonetaryValue] / T.[AlgebraicQuantity] <> T.[PriorMVPU]
				OR	T.[AlgebraicValue] / T.[AlgebraicQuantity] <> T.[PriorVPU])
			GROUP BY T.[AccountId], T.[CenterId], T.[CustodyId], T.[ResourceId]
		)
		-- Update all the smart inventory issues in between with the prior CPU
		UPDATE T
		SET 
			T.[AlgebraicMonetaryValue] = T.[AlgebraicQuantity] * T.[PriorMVPU],
			T.[AlgebraicValue] = T.[AlgebraicQuantity] * T.[PriorVPU]
		FROM @T T
		JOIN BatchStartAndVPU BS ON T.[AccountId] = BS.[AccountId] AND T.[CenterId] = BS.[CenterId] AND T.[CustodyId] = BS.[CustodyId] AND T.[ResourceId] = BS.[ResourceId]
		LEFT JOIN BatchEnd BE ON T.[AccountId] = BE.[AccountId] AND T.[CenterId] = BE.[CenterId] AND T.[CustodyId] = BE.[CustodyId] AND T.[ResourceId] = BE.[ResourceId]
		WHERE T.[PostingDate] > BS.[PostingDate] AND (BE.[PostingDate] IS NULL OR T.[PostingDate] < BE.[PostingDate])
		AND (T.[AlgebraicMonetaryValue] <> T.[AlgebraicQuantity] * T.[PriorMVPU] OR T.[AlgebraicValue] <> T.[AlgebraicQuantity] * T.[PriorVPU]);
		SET @RowCount = @@ROWCOUNT;
		-- IF no changes, exit the loop
		IF @RowCount = 0 BREAK;
		IF @RowCount = @PrevRowCount -- Stuck in a loop
		BEGIN
			RAISERROR(N'Stuck in infinite loop, @RowCount = %d, @PrevRowCount = %d, @LoopCounter = %d', 16, 1,
						@RowCount, @PrevRowCount, @LoopCounter)
			BREAK;
		END;
	
		WITH CumBalances AS (
			SELECT [Id],
				SUM([AlgebraicMonetaryValue]) OVER (Partition BY [AccountId], [CenterId], [CustodyId], [ResourceId] ORDER BY [PostingDate], [Direction] DESC) AS RunningMonetaryValue,
				SUM([AlgebraicValue]) OVER (Partition BY [AccountId], [CenterId], [CustodyId], [ResourceId] ORDER BY [PostingDate], [Direction] DESC) AS RunningValue
			FROM @T
		)
		UPDATE T
		SET
			T.RunningMonetaryValue = CB.RunningMonetaryValue,
			T.RunningValue = CB.RunningValue
		FROM @T T
		JOIN CumBalances CB ON T.[Id] = CB.[Id]
	END

SELECT T.Id, T.AccountId, W.[Name] As Warehouse, R.[Name] As [Resource], PostingDate, Direction,
AlgebraicQuantity, AlgebraicMonetaryValue 	, 
AlgebraicValue ,	RunningQuantity,	 RunningMonetaryValue, 	 RunningValue ,	 PriorMVPU, 	 PriorVPU 
FROM @T T
LEFT JOIN dbo.Custodies W ON T.CustodyId = W.Id
join dbo.resources R ON T.ResourceID = R.Id
order by T.[AccountId], T.[CustodyId], T.[ResourceId], T.PostingDate, T.Direction Desc

/*
UPDATE E
SET
	E.[MonetaryValue] = ABS(T.[AlgebraicMonetaryValue]),
	E.[Value] = ABS(T.[AlgebraicValue])
FROM dbo.Entries E
JOIN dbo.Lines L ON L.[Id] = E.[LineId]
JOIN @T T ON T.[AccountId] = E.AccountId AND T.[CustodyId] = E.[CustodyId] AND T.[ResourceId] = E.[ResourceId] AND T.[PostingDate] = L.[PostingDate]
JOIN @AffectedLineDefinitionEntries LD ON LD.LineDefinitionId = L.[DefinitionId] AND LD.[Index] = E.[Index]
WHERE (E.[MonetaryValue] <> ABS(T.[AlgebraicMonetaryValue]) OR E.[Value] <> ABS(T.[AlgebraicValue]));
*/