CREATE PROCEDURE [dal].[InventoryEntries_AVCO__Update] -- [dal].[InventoryEntries_AVCO__Update] 0
@VerifyLineDefinitions BIT = 1
AS
	DECLARE @AffectedLineDefinitionEntries TABLE (
		[LineDefinitionId] INT,
		[Index] INT
		PRIMARY KEY ([LineDefinitionId], [Index])
	);

	DECLARE @T TABLE (
		[Id]					INT PRIMARY KEY IDENTITY,
		[PostingDate]			DATE,
		[LineId]				INT,
		[Direction]				SMALLINT,
		[CustodyId]				INT,
		[ResourceId]			INT,
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
	DECLARE @ManualLine INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
	SET NOCOUNT ON;
	-- Look for inventory credit smart entries
	WITH InventoryAccountTypes AS (
		SELECT ATC.[Id]
		FROM dbo.AccountTypes ATC
		JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node])  = 1
		WHERE ATP.[Concept] = N'Inventories' AND ATC.Concept <> N'CurrentInventoriesInTransit'
	)
	INSERT INTO @AffectedLineDefinitionEntries([LineDefinitionId], [Index])
	SELECT [LineDefinitionId], [Index]
	FROM dbo.LineDefinitionEntries
	WHERE [ParentAccountTypeId] IN (SELECT [Id] FROM InventoryAccountTypes)
	AND [Direction] = -1;
	-- Assume the debit entry comes before it (we test the assumption below)
	INSERT INTO @AffectedLineDefinitionEntries([LineDefinitionId], [Index])
	SELECT [LineDefinitionId], [Index] - 1
	FROM @AffectedLineDefinitionEntries;
	-- Check if posted entries are balances already
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

		RAISERROR(@BadLineDefinition, 16, 1)
		RETURN
	END;
	-- Initial conditions: look for inventory entries
	WITH InventoryAccountTypes AS (
		SELECT ATC.[Id]
		FROM dbo.AccountTypes ATC
		JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node])  = 1
		WHERE ATP.[Concept] = N'Inventories' AND ATC.Concept <> N'CurrentInventoriesInTransit'
	),
	InventoryAccounts AS (
		SELECT A.[Id]
		FROM dbo.Accounts A
		WHERE AccountTypeId IN (SELECT [Id] FROM InventoryAccountTypes)
	)
	INSERT INTO @T([PostingDate], [LineId], [Direction], [CustodyId], [ResourceId],
				[AlgebraicQuantity], [AlgebraicMonetaryValue], [AlgebraicValue],
				[RunningQuantity], [RunningMonetaryValue], [RunningValue])
	SELECT L.PostingDate, L.[Id], E.[Direction], E.[CustodyId], E.[ResourceId],
		E.[AlgebraicQuantity], E.[AlgebraicMonetaryValue], E.[AlgebraicValue],
			SUM([AlgebraicQuantity]) OVER (Partition BY  [ResourceId], [CustodyId] ORDER BY [PostingDate], [LineId]) AS RunningQuantity,
			SUM([AlgebraicMonetaryValue]) OVER (Partition BY [CustodyId], [ResourceId] ORDER BY [PostingDate], [LineId]) AS RunningMonetaryValue,
			SUM([AlgebraicValue]) OVER (Partition BY [CustodyId], [ResourceId] ORDER BY [PostingDate], [LineId]) AS RunningValue
	FROM map.DetailsEntries() E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	WHERE AccountId IN (SELECT [Id] FROM InventoryAccounts)
	AND L.[State] = 4
	ORDER BY L.PostingDate, L.Id, Direction Desc;
	
	DECLARE @RowCount INT = -1, @PrevRowCount INT = -1, @LoopCounter INT = 0;
	WHILE (1 = 1)
	BEGIN -- Loop to calculate AVCO
		SET @PrevRowCount = @RowCount; SET @LoopCounter = @LoopCounter + 1;
		UPDATE @T
		SET
			PriorMVPU = IIF([RunningQuantity]=[AlgebraicQuantity],0,([RunningMonetaryValue] - [AlgebraicMonetaryValue]) / ([RunningQuantity] - [AlgebraicQuantity])),
			PriorVPU =  IIF([RunningQuantity]=[AlgebraicQuantity],0,([RunningValue] - [AlgebraicValue]) /  ([RunningQuantity] - [AlgebraicQuantity]));
		-- Look for first smart issue where the CPU has deviated from Prior CPU
		WITH BatchStartAndVPU AS (
			SELECT MIN([Id]) As Id, [CustodyId], [ResourceId]
			FROM @T T
			WHERE [Direction] = -1
			AND [LineId] NOT IN (SELECT [Id] FROM dbo.Lines WHERE DefinitionId = @ManualLine)
			AND (T.[AlgebraicMonetaryValue] / T.[AlgebraicQuantity] <> T.[PriorMVPU]
				OR	T.[AlgebraicValue] / T.[AlgebraicQuantity] <> T.[PriorVPU])
			GROUP BY [CustodyId], [ResourceId]
		),
		-- Look for first receipt (smart or JV) where the CPU has deviated from Prior CPU
		BatchEnd AS (
			SELECT
				MIN(T.[Id]) As Id, T.[CustodyId], T.[ResourceId]
			FROM @T AS T
			JOIN BatchStartAndVPU BS ON T.[CustodyId] = BS.[CustodyId] AND T.[ResourceId] = BS.[ResourceId]
			WHERE [Direction] = 1
			AND T.[Id] > BS.[Id]
			AND (T.[AlgebraicMonetaryValue] / T.[AlgebraicQuantity] <> T.[PriorMVPU]
				OR	T.[AlgebraicValue] / T.[AlgebraicQuantity] <> T.[PriorVPU])
			GROUP BY T.[CustodyId], T.[ResourceId]
		)
		-- Update all the smart inventory issues in between with the prior CPU
		UPDATE T
		SET 
			T.[AlgebraicMonetaryValue] = T.[AlgebraicQuantity] * T.[PriorMVPU],
			T.[AlgebraicValue] = T.[AlgebraicQuantity] * T.[PriorVPU]
		FROM @T T
		JOIN BatchStartAndVPU BS ON T.[CustodyId] = BS.[CustodyId] AND T.[ResourceId] = BS.[ResourceId]
		LEFT JOIN BatchEnd BE ON T.[CustodyId] = BE.[CustodyId] AND T.[ResourceId] = BE.[ResourceId]
		WHERE T.[Id] >= BS.[Id] AND (BE.[Id] IS NULL OR T.[Id] < BE.[Id])
		AND T.[LineId] NOT IN (SELECT [Id] FROM dbo.Lines WHERE DefinitionId = @ManualLine)
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
				SUM([AlgebraicMonetaryValue]) OVER (Partition BY [CustodyId], [ResourceId] ORDER BY [PostingDate], [LineId]) AS RunningMonetaryValue,
				SUM([AlgebraicValue]) OVER (Partition BY [CustodyId], [ResourceId] ORDER BY [PostingDate], [LineId]) AS RunningValue
			FROM @T
		)
		UPDATE T
		SET
			T.RunningMonetaryValue = CB.RunningMonetaryValue,
			T.RunningValue = CB.RunningValue
		FROM @T T
		JOIN CumBalances CB ON T.[Id] = CB.[Id]
	END

UPDATE E
SET
	E.[MonetaryValue] = -T.[AlgebraicMonetaryValue],
	E.[Value] = -T.[AlgebraicValue]
FROM dbo.Entries E
JOIN dbo.Lines L ON L.[Id] = E.[LineId]
JOIN @T T ON T.[LineId] = E.[LineId]
JOIN @AffectedLineDefinitionEntries LD ON LD.LineDefinitionId = L.[DefinitionId] AND LD.[Index] = E.[Index]
WHERE (E.[MonetaryValue] <> -T.[AlgebraicMonetaryValue] OR E.[Value] <> -T.[AlgebraicValue]);