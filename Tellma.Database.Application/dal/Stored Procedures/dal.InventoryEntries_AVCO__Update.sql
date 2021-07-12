/****** Object:  StoredProcedure [dal].[InventoryEntries_AVCO__Update]    Script Date: 6/22/2021 5:43:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dal].[InventoryEntries_AVCO__Update]
@ArchiveDate DATE = N'2020.07.07',
@VerifyLineDefinitions BIT = 0
AS
	DECLARE @Epsilon DECIMAL (19,4) = 0.0001;

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
		[Direction]				SMALLINT,-- INDEX IX_Direction([Direction]),
		INDEX IX_T UNIQUE CLUSTERED([AccountId], [CenterId], [CustodyId], [ResourceId], [PostingDate], [Direction] DESC),
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
	Declare @StartTime1 DateTime2 = SysUTCDateTime();
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
--		JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
		WHERE E.[AccountId] IN (SELECT [Id] FROM InventoryAccounts)
		AND L.[State] = 4
--		AND DD.[DocumentType] = 2
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
	Print '1: Time taken was ' + cast(DateDiff(millisecond, @StartTime1, SysUTCDateTime()) as varchar) + 'ms'

	DECLARE @LoopCounter INT = 0;
	Declare @StartTime2 DateTime2 = SysUTCDateTime();
	WHILE (1 = 1)
	BEGIN -- Loop to calculate AVCO

		SET @LoopCounter = @LoopCounter + 1;
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
		DECLARE @BatchStartAndVPU TABLE (
			[Id]					INT PRIMARY KEY IDENTITY,	
			[AccountId]				INT,
			[CenterId]				INT,
			[CustodyId]				INT,
			[ResourceId]			INT,
			[PostingDate]			DATE,
			[MVPU]					FLOAT (53) DEFAULT (0),
			[VPU]					FLOAT (53) DEFAULT (0),
			INDEX IX_BS UNIQUE CLUSTERED([AccountId], [CenterId], [CustodyId], [ResourceId], [PostingDate])
		);
		DECLARE @BatchEnd TABLE (
			[Id]					INT PRIMARY KEY IDENTITY,	
			[AccountId]				INT,
			[CenterId]				INT,
			[CustodyId]				INT,
			[ResourceId]			INT,
			[PostingDate]			DATE,
			INDEX IX_BE UNIQUE CLUSTERED([AccountId], [CenterId], [CustodyId], [ResourceId], [PostingDate])
		);

		--WITH BatchStartAndVPU AS (
		--	SELECT T.[AccountId], T.[CenterId], T.[CustodyId], T.[ResourceId], MIN(T.[PostingDate]) As [PostingDate]
		--	FROM @T T
		--	WHERE T.[Direction] = -1
		--	AND (T.[AlgebraicMonetaryValue] <> T.[PriorMVPU] * T.[AlgebraicQuantity]
		--		OR	T.[AlgebraicValue] <> T.[PriorVPU] * T.[AlgebraicQuantity])
		--	GROUP BY T.[AccountId], T.[CenterId], T.[CustodyId], T.[ResourceId]
		--),
		---- Look for first date (smart or JV) where the CPU has deviated from Prior CPU
		--BatchEnd AS (
		--	SELECT T.[AccountId], T.[CenterId], T.[CustodyId], T.[ResourceId], MIN(T.[PostingDate]) As [PostingDate]
		--	FROM @T T
		--	JOIN BatchStartAndVPU BS ON T.[AccountId] = BS.[AccountId] AND T.[CenterId] = BS.[CenterId]
		--		AND T.[CustodyId] = BS.[CustodyId] AND T.[ResourceId] = BS.[ResourceId]
		--	WHERE T.[Direction] = +1
		--	AND T.[PostingDate] > BS.[PostingDate]
		--	AND (T.[AlgebraicMonetaryValue] <> T.[PriorMVPU] * T.[AlgebraicQuantity]
		--		OR	T.[AlgebraicValue] <> T.[PriorVPU] * T.[AlgebraicQuantity])
		--	GROUP BY T.[AccountId], T.[CenterId], T.[CustodyId], T.[ResourceId]
		--)
		DELETE @BatchStartAndVPU;
		INSERT INTO @BatchStartAndVPU([AccountId], [CenterId], [CustodyId], [ResourceId], [PostingDate])
		SELECT T.[AccountId], T.[CenterId], T.[CustodyId], T.[ResourceId], MIN(T.[PostingDate]) As [PostingDate]
		FROM @T T
		WHERE T.[Direction] = -1
		AND ABS(T.[AlgebraicValue] - T.[PriorVPU] * T.[AlgebraicQuantity]) > @Epsilon
		GROUP BY T.[AccountId], T.[CenterId], T.[CustodyId], T.[ResourceId];
		
		UPDATE BS
		SET
			BS.[MVPU] = T.[PriorMVPU],
			BS.[VPU] = T.[PriorVPU]
		FROM @BatchStartAndVPU BS
		JOIN @T T ON T.[AccountId] = BS.[AccountId] AND T.[CenterId] = BS.[CenterId] AND T.[CustodyId] = BS.[CustodyId] AND T.[ResourceId] = BS.[ResourceId] AND T.[PostingDate] = BS.[PostingDate]
		WHERE T.[Direction] = -1

		DELETE @BatchEnd;
		INSERT INTO @BatchEnd([AccountId], [CenterId], [CustodyId], [ResourceId], [PostingDate])
		SELECT T.[AccountId], T.[CenterId], T.[CustodyId], T.[ResourceId], MIN(T.[PostingDate]) As [PostingDate]
		FROM @T T
		JOIN @BatchStartAndVPU BS ON T.[AccountId] = BS.[AccountId] AND T.[CenterId] = BS.[CenterId]
			AND T.[CustodyId] = BS.[CustodyId] AND T.[ResourceId] = BS.[ResourceId]
		WHERE T.[Direction] = +1
		AND T.[PostingDate] > BS.[PostingDate]
		AND (ABS(T.[AlgebraicMonetaryValue] - T.[PriorMVPU] * T.[AlgebraicQuantity]) > @Epsilon
			OR	ABS(T.[AlgebraicValue] - T.[PriorVPU] * T.[AlgebraicQuantity]) > @Epsilon )
		GROUP BY T.[AccountId], T.[CenterId], T.[CustodyId], T.[ResourceId]

		-- Update all the smart inventory issues in between with the prior CPU
		UPDATE T
		SET 
			T.[AlgebraicMonetaryValue] = T.[AlgebraicQuantity] * BS.[MVPU],
			T.[AlgebraicValue] = T.[AlgebraicQuantity] * BS.[VPU]
		FROM @T T
		JOIN @BatchStartAndVPU BS ON T.[AccountId] = BS.[AccountId] AND T.[CenterId] = BS.[CenterId] AND T.[CustodyId] = BS.[CustodyId] AND T.[ResourceId] = BS.[ResourceId]
		LEFT JOIN @BatchEnd BE ON T.[AccountId] = BE.[AccountId] AND T.[CenterId] = BE.[CenterId] AND T.[CustodyId] = BE.[CustodyId] AND T.[ResourceId] = BE.[ResourceId]
		WHERE T.[PostingDate] >= BS.[PostingDate]
		AND (BE.[PostingDate] IS NULL OR T.[PostingDate] < BE.[PostingDate])
		AND (ABS(T.[AlgebraicValue] - T.[PriorVPU] * T.[AlgebraicQuantity]) > @Epsilon )	
		AND T.[Direction] = -1;
		-- IF no changes, exit the loop
		IF @@ROWCOUNT = 0 BREAK;

		IF @loopCounter > 366 -- worst case can happen when we buy daily at different price, and sell daily as well.
		BEGIN
			RAISERROR(N'Taking too long, @LoopCounter = %d', 16, 1, @LoopCounter)
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
		JOIN CumBalances CB ON T.[Id] = CB.[Id];		
	END;
	Print '2: Time taken was ' + cast(DateDiff(millisecond, @StartTime2, SysUTCDateTime()) as varchar) + 'ms'

Declare @StartTime3 DateTime2 = SysUTCDateTime();
WITH NewValues AS (
	SELECT E.[LineId], E.[Index], 
			ROUND(ABS(T.[AlgebraicMonetaryValue] * E.[BaseQuantity] / T.[AlgebraicQuantity]), 2) AS NewMonetaryValue, 
			ROUND(ABS(T.[AlgebraicValue] * E.[BaseQuantity] / T.[AlgebraicQuantity]), 2) AS NewValue
	FROM map.DetailsEntries() E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN @T T ON T.[AccountId] = E.AccountId AND T.[CenterId] = E.[CenterId] AND T.[CustodyId] = E.[CustodyId] AND T.[ResourceId] = E.[ResourceId] AND T.[PostingDate] = L.[PostingDate]
	JOIN @AffectedLineDefinitionEntries LDE ON LDE.LineDefinitionId = L.[DefinitionId] AND LDE.[Index] = E.[Index]
	WHERE T.[AlgebraicQuantity] <> 0
	--AND E.[Value] <> ROUND(ABS(T.[AlgebraicValue] * E.[BaseQuantity] / T.[AlgebraicQuantity]), 2)
	--AND ABS(ABS(E.[Value] * T.[AlgebraicQuantity]) - ABS(T.[AlgebraicValue] * E.[BaseQuantity])) > @Epsilon
	AND T.[Direction] = -1 AND E.[Direction] = -1
)
UPDATE E
SET
	E.[MonetaryValue]	= NV.[NewMonetaryValue],
	E.[Value]			= NV.[NewValue]
FROM dbo.Entries E
JOIN NewValues NV ON E.[LineId] = NV.LineId AND (E.[Index] = NV.[Index] OR E.[Index] = NV.[Index] - 1)

Print '3: Time taken was ' + cast(DateDiff(millisecond, @StartTime3, SysUTCDateTime()) as varchar) + 'ms'
DONE: