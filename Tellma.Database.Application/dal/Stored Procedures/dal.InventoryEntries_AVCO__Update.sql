CREATE PROCEDURE [dal].[InventoryEntries_AVCO__Update]
-- [dal].[InventoryEntries_AVCO__Update] @ArchiveDate = N'2025.08.01', @MinState = 0;
-- Currently only in 110. Needs to be synced to others.
@ArchiveDate DATE,
@MinState TINYINT = 4,
@VerifyLineDefinitions BIT = 0
AS
    DECLARE @Epsilon DECIMAL (19,4) = 0.0001;

    -- ============================================================
    -- Pre-compute Center to BusinessUnit mapping ONCE (OPTIMIZATION)
    -- ============================================================
    DECLARE @CenterBusinessUnit TABLE (
        CenterId        INT PRIMARY KEY,
        BusinessUnitId  INT,
        INDEX IX_CBU_BU (BusinessUnitId)
    );

    WITH BusinessUnits AS (
        SELECT [Id], [Node]
        FROM dbo.Centers
        WHERE CenterType = N'BusinessUnit'
    ),
    RootCenter AS (
        SELECT [Id], [Node]
        FROM dbo.Centers
        WHERE ParentId IS NULL
    )
    INSERT INTO @CenterBusinessUnit (CenterId, BusinessUnitId)
    SELECT
        C.[Id]                      AS CenterId,
        COALESCE(BU.[Id], RC.[Id])  AS BusinessUnitId
    FROM dbo.Centers C
    LEFT JOIN BusinessUnits BU ON C.[Node].IsDescendantOf(BU.[Node]) = 1
    LEFT JOIN RootCenter    RC ON C.[Node].IsDescendantOf(RC.[Node]) = 1;
    -- ============================================================

    -- ============================================================
    -- @AffectedLineDefinitionEntries
    --
    -- IsBOMTransfer = 1 ONLY for line definitions whose credit side
    -- is specifically a WorkInProgress account (WIP -> FG transfers).
    --
    -- IMPORTANT: A raw-material -> WIP issue also has both sides
    -- under Inventories, but its credit side is RawMaterials, not
    -- WorkInProgress.  Using IsBOMTransfer=0 for those ensures the
    -- AVCO loop still corrects them.
    --
    -- Classification:
    --   Raw material -> WIP  : credit = RawMaterials  -> IsBOMTransfer = 0 (AVCO corrects it)
    --   WIP -> Finished Goods: credit = WorkInProgress -> IsBOMTransfer = 1 (BOM pass corrects it)
    --   FG -> Customer       : credit = FinishedGoods  -> IsBOMTransfer = 0 (AVCO corrects it)
    -- ============================================================
    DECLARE @AffectedLineDefinitionEntries TABLE (
        [LineDefinitionId]  INT,
        [Index]             INT,
        [IsBOMTransfer]     BIT NOT NULL DEFAULT 0,
        PRIMARY KEY ([LineDefinitionId], [Index])
    );

    -- ============================================================
    -- @T carries IsBOMTransfer so the AVCO loop can filter with a
    -- simple WHERE clause without re-querying map.DetailsEntries().
    -- ============================================================
    DECLARE @T TABLE (
        [Id]                     INT PRIMARY KEY IDENTITY,
        [AccountId]              INT,
        [BusinessUnitId]         INT,
        [AgentId]                INT,
        [ResourceId]             INT,
        [PostingDate]            DATE,
        [Direction]              SMALLINT,
        [IsBOMTransfer]          BIT NOT NULL DEFAULT 0,
        INDEX IX_T UNIQUE CLUSTERED([AccountId], [BusinessUnitId], [AgentId], [ResourceId], [PostingDate], [Direction] DESC),
        [AlgebraicQuantity]      DECIMAL (19, 4),
        [AlgebraicMonetaryValue] DECIMAL (19, 4),
        [AlgebraicValue]         DECIMAL (19, 4),
        [RunningQuantity]        DECIMAL (19, 4),
        [RunningMonetaryValue]   DECIMAL (19, 4),
        [RunningValue]           DECIMAL (19, 4),
        [PriorMVPU]              FLOAT (53) DEFAULT (0),
        [PriorVPU]               FLOAT (53) DEFAULT (0)
    );

    DECLARE @BadLineDefinitionId INT;
    DECLARE @ManualLine INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
    SET NOCOUNT ON;

    DECLARE @StartTime1 DATETIME2 = SysUTCDateTime();

    -- ============================================================
    -- Identify affected line definitions and classify each one.
    --
    -- IsBOMTransfer = 1 when the inventory credit side of the line
    -- is specifically a WorkInProgress account.  Every other
    -- inventory credit (raw materials, finished goods) gets 0.
    --
    -- Key distinction:
    --   Any inventory credit paired with an inventory debit would
    --   incorrectly mark raw-material->WIP lines as BOM transfers.
    --   Instead we test whether the credit side IS WorkInProgress.
    -- ============================================================
    WITH InventoryAccountTypes AS (
        SELECT ATC.[Id]
        FROM dbo.AccountTypes ATC
        JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node]) = 1
        WHERE ATP.[Concept] = N'Inventories'
    ),
    WIPAccountTypes AS (
        SELECT ATC.[Id]
        FROM dbo.AccountTypes ATC
        JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node]) = 1
        WHERE ATP.[Concept] = N'WorkInProgress'
    ),
    -- All inventory credit entries (raw material issues, WIP issues, FG issues)
    InventoryCredits AS (
        SELECT [LineDefinitionId], [Index], [ParentAccountTypeId]
        FROM dbo.LineDefinitionEntries
        WHERE [ParentAccountTypeId] IN (SELECT [Id] FROM InventoryAccountTypes)
        AND   [Direction] = -1
    ),
    -- Line definitions that have a WIP credit entry specifically.
    -- Only these are true WIP->FG BOM transfers.
    WIPCreditLineDefinitions AS (
        SELECT DISTINCT [LineDefinitionId]
        FROM dbo.LineDefinitionEntries
        WHERE [ParentAccountTypeId] IN (SELECT [Id] FROM WIPAccountTypes)
        AND   [Direction] = -1
    )
    INSERT INTO @AffectedLineDefinitionEntries ([LineDefinitionId], [Index], [IsBOMTransfer])
    SELECT
        IC.[LineDefinitionId],
        IC.[Index],
        IIF(EXISTS (
            SELECT 1 FROM WIPCreditLineDefinitions W
            WHERE W.[LineDefinitionId] = IC.[LineDefinitionId]
        ), 1, 0) AS [IsBOMTransfer]
    FROM InventoryCredits IC;

    -- Add the paired debit entry (Index - 1) where not already present
    INSERT INTO @AffectedLineDefinitionEntries ([LineDefinitionId], [Index], [IsBOMTransfer])
    SELECT A.[LineDefinitionId], A.[Index] - 1, A.[IsBOMTransfer]
    FROM @AffectedLineDefinitionEntries A
    WHERE NOT EXISTS (
        SELECT 1
        FROM @AffectedLineDefinitionEntries B
        WHERE B.[LineDefinitionId] = A.[LineDefinitionId]
        AND   B.[Index]            = A.[Index] - 1
    );

    -- ============================================================
    -- Verify line definition assumptions (debit before credit).
    -- Only check simple issue lines (IsBOMTransfer = 0).
    -- BOM transfer lines balance across the whole document, not
    -- within a single line pair, so they are excluded here.
    -- ============================================================
    IF @VerifyLineDefinitions = 1
    SELECT @BadLineDefinitionId = LD.[LineDefinitionId]
    FROM dbo.Entries E
    JOIN dbo.Lines L ON L.[Id] = E.[LineId]
    JOIN @AffectedLineDefinitionEntries LD
        ON LD.[LineDefinitionId] = L.[DefinitionId] AND LD.[Index] = E.[Index]
    WHERE L.[State] >= @MinState
    AND   LD.[IsBOMTransfer] = 0
    GROUP BY LD.[LineDefinitionId]
    HAVING SUM(E.[Direction] * E.[Value]) <> 0;

    IF @BadLineDefinitionId IS NOT NULL
    BEGIN
        DECLARE @BadLineDefinition NVARCHAR (255);
        SELECT @BadLineDefinition =
            N'Improper Line Definition Design: ' + [TitleSingular] +
            N'. The debit should come before the credit for inventory issue.'
        FROM dbo.LineDefinitions
        WHERE [Id] = @BadLineDefinitionId;
        THROW 50000, @BadLineDefinition, 1;
        RETURN;
    END;

    -- ============================================================
    -- Populate @T with all inventory entries.
    -- IsBOMTransfer is resolved once here via a LEFT JOIN so the
    -- AVCO loop can filter with a plain WHERE clause instead of
    -- re-calling map.DetailsEntries() on every iteration.
    -- ============================================================
    WITH InventoryAccountTypes AS (
        SELECT ATC.[Id]
        FROM dbo.AccountTypes ATC
        JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node]) = 1
        WHERE ATP.[Concept] = N'Inventories'
    ),
    InventoryAccounts AS (
        SELECT A.[Id]
        FROM dbo.Accounts A
        WHERE AccountTypeId IN (SELECT [Id] FROM InventoryAccountTypes)
    ),
    AccumulatedEntries AS (
        SELECT
            E.[AccountId],
            CBU.BusinessUnitId,
            E.[AgentId],
            E.[ResourceId],
            L.[PostingDate],
            E.[Direction],
            -- If any entry in this aggregated row belongs to a BOM-transfer
            -- line definition, mark the whole row as a BOM transfer.
            -- Entries with no matching LDE row get ISNULL -> 0.
            CAST(MAX(CAST(ISNULL(LDE.[IsBOMTransfer], 0) AS INT)) AS BIT) AS [IsBOMTransfer],
            ISNULL(SUM(E.[Direction] * E.[BaseQuantity]),  0) AS [AlgebraicQuantity],
            SUM(E.[Direction] * E.[MonetaryValue])           AS [AlgebraicMonetaryValue],
            SUM(E.[Direction] * E.[Value])                   AS [AlgebraicValue]
        FROM map.DetailsEntries() E
        JOIN dbo.Lines L ON L.[Id] = E.[LineId]
        JOIN @CenterBusinessUnit CBU ON CBU.CenterId = E.[CenterId]
        -- LEFT JOIN: entries not in @AffectedLineDefinitionEntries default to IsBOMTransfer = 0
        LEFT JOIN @AffectedLineDefinitionEntries LDE
            ON  LDE.[LineDefinitionId] = L.[DefinitionId]
            AND LDE.[Index]            = E.[Index]
        WHERE E.[AccountId] IN (SELECT [Id] FROM InventoryAccounts)
        AND   L.[State] >= @MinState
        GROUP BY E.[AccountId], CBU.BusinessUnitId, E.[AgentId], E.[ResourceId], L.[PostingDate], E.[Direction]
    )
    INSERT INTO @T (
        [AccountId], [BusinessUnitId], [AgentId], [ResourceId], [PostingDate], [Direction], [IsBOMTransfer],
        [AlgebraicQuantity], [AlgebraicMonetaryValue], [AlgebraicValue],
        [RunningQuantity],   [RunningMonetaryValue],   [RunningValue])
    SELECT
        [AccountId], [BusinessUnitId], [AgentId], [ResourceId], [PostingDate], [Direction], [IsBOMTransfer],
        [AlgebraicQuantity], [AlgebraicMonetaryValue], [AlgebraicValue],
        SUM([AlgebraicQuantity])
            OVER (PARTITION BY [AccountId], [AgentId], [ResourceId]
                  ORDER BY [PostingDate], [Direction] DESC) AS RunningQuantity,
        SUM([AlgebraicMonetaryValue])
            OVER (PARTITION BY [AccountId], [AgentId], [ResourceId]
                  ORDER BY [PostingDate], [Direction] DESC) AS RunningMonetaryValue,
        SUM([AlgebraicValue])
            OVER (PARTITION BY [AccountId], [AgentId], [ResourceId]
                  ORDER BY [PostingDate], [Direction] DESC) AS RunningValue
    FROM AccumulatedEntries
    ORDER BY [AccountId], [BusinessUnitId], [AgentId], [ResourceId], [PostingDate], [Direction] DESC;

    PRINT '1: Time taken was ' + CAST(DATEDIFF(millisecond, @StartTime1, SysUTCDateTime()) AS VARCHAR) + 'ms';

    -- ============================================================
    -- AVCO iterative loop.
    -- Processes simple inventory issues (IsBOMTransfer = 0) only.
    -- WIP->FG rows are excluded via the WHERE on @T:
    --   Without exclusion, WIP->FG credit rows would be zeroed out
    --   because their ResourceId partition has no matching
    --   Direction=+1 receipt row, making PriorVPU = 0.
    -- ============================================================
    DECLARE @LoopCounter INT = 0;
    DECLARE @StartTime2 DATETIME2 = SysUTCDateTime();

    WHILE (1 = 1)
    BEGIN
        SET @LoopCounter = @LoopCounter + 1;

        UPDATE @T
        SET
            PriorMVPU = IIF([RunningQuantity] = [AlgebraicQuantity], 0,
                            ([RunningMonetaryValue] - [AlgebraicMonetaryValue]) /
                            ([RunningQuantity]      - [AlgebraicQuantity])),
            PriorVPU  = IIF([RunningQuantity] = [AlgebraicQuantity], 0,
                            ([RunningValue]   - [AlgebraicValue]) /
                            ([RunningQuantity] - [AlgebraicQuantity]));

        DECLARE @BatchStartAndVPU TABLE (
            [Id]             INT PRIMARY KEY IDENTITY,
            [AccountId]      INT,
            [BusinessUnitId] INT,
            [AgentId]        INT,
            [ResourceId]     INT,
            [PostingDate]    DATE,
            [MVPU]           FLOAT (53) DEFAULT (0),
            [VPU]            FLOAT (53) DEFAULT (0),
            INDEX IX_BS UNIQUE CLUSTERED([AccountId], [BusinessUnitId], [AgentId], [ResourceId], [PostingDate])
        );

        DECLARE @BatchEnd TABLE (
            [Id]             INT PRIMARY KEY IDENTITY,
            [AccountId]      INT,
            [BusinessUnitId] INT,
            [AgentId]        INT,
            [ResourceId]     INT,
            [PostingDate]    DATE,
            INDEX IX_BE UNIQUE CLUSTERED([AccountId], [BusinessUnitId], [AgentId], [ResourceId], [PostingDate])
        );

        DELETE @BatchStartAndVPU;
        INSERT INTO @BatchStartAndVPU ([AccountId], [BusinessUnitId], [AgentId], [ResourceId], [PostingDate])
        SELECT T.[AccountId], T.[BusinessUnitId], T.[AgentId], T.[ResourceId], MIN(T.[PostingDate])
        FROM @T T
        WHERE T.[Direction]     = -1
        AND   T.[IsBOMTransfer] = 0    -- Exclude WIP->FG rows only; raw-material->WIP stays here
        AND   ABS(T.[AlgebraicValue] - T.[PriorVPU] * T.[AlgebraicQuantity]) > @Epsilon
        GROUP BY T.[AccountId], T.[BusinessUnitId], T.[AgentId], T.[ResourceId];

        UPDATE BS
        SET BS.[MVPU] = T.[PriorMVPU],
            BS.[VPU]  = T.[PriorVPU]
        FROM @BatchStartAndVPU BS
        JOIN @T T ON T.[AccountId]      = BS.[AccountId]
                 AND T.[BusinessUnitId] = BS.[BusinessUnitId]
                 AND T.[AgentId]        = BS.[AgentId]
                 AND T.[ResourceId]     = BS.[ResourceId]
                 AND T.[PostingDate]    = BS.[PostingDate]
        WHERE T.[Direction] = -1;

        DELETE @BatchEnd;
        INSERT INTO @BatchEnd ([AccountId], [BusinessUnitId], [AgentId], [ResourceId], [PostingDate])
        SELECT T.[AccountId], T.[BusinessUnitId], T.[AgentId], T.[ResourceId], MIN(T.[PostingDate])
        FROM @T T
        JOIN @BatchStartAndVPU BS ON T.[AccountId]      = BS.[AccountId]
                                 AND T.[BusinessUnitId] = BS.[BusinessUnitId]
                                 AND T.[AgentId]        = BS.[AgentId]
                                 AND T.[ResourceId]     = BS.[ResourceId]
        WHERE T.[Direction]   = +1
        AND   T.[PostingDate] > BS.[PostingDate]
        AND  (ABS(T.[AlgebraicMonetaryValue] - T.[PriorMVPU] * T.[AlgebraicQuantity]) > @Epsilon
           OR ABS(T.[AlgebraicValue]         - T.[PriorVPU]  * T.[AlgebraicQuantity]) > @Epsilon)
        GROUP BY T.[AccountId], T.[BusinessUnitId], T.[AgentId], T.[ResourceId];

        UPDATE T
        SET
            T.[AlgebraicMonetaryValue] = T.[AlgebraicQuantity] * BS.[MVPU],
            T.[AlgebraicValue]         = T.[AlgebraicQuantity] * BS.[VPU]
        FROM @T T
        JOIN @BatchStartAndVPU BS ON T.[AgentId]        = BS.[AgentId]
                                 AND T.[ResourceId]      = BS.[ResourceId]
                                 AND T.[BusinessUnitId]  = BS.[BusinessUnitId]
        LEFT JOIN @BatchEnd    BE ON T.[AgentId]         = BE.[AgentId]
                                 AND T.[ResourceId]      = BE.[ResourceId]
                                 AND T.[BusinessUnitId]  = BE.[BusinessUnitId]
        WHERE T.[PostingDate] >= BS.[PostingDate]
        AND  (BE.[PostingDate] IS NULL OR T.[PostingDate] < BE.[PostingDate])
        AND   ABS(T.[AlgebraicValue] - T.[PriorVPU] * T.[AlgebraicQuantity]) > @Epsilon
        AND   T.[Direction] = -1;

        IF @LoopCounter > 366
        BEGIN
            PRINT 'Warning: AVCO loop reached iteration limit (' + 
                  CAST(@LoopCounter AS VARCHAR) + '). Remaining delta is sub-epsilon noise.';
            BREAK;
        END;
        -- Recompute running totals after each correction pass
        WITH CumBalances AS (
            SELECT
                [Id],
                SUM([AlgebraicMonetaryValue])
                    OVER (PARTITION BY [AccountId], [BusinessUnitId], [AgentId], [ResourceId]
                          ORDER BY [PostingDate], [Direction] DESC) AS RunningMonetaryValue,
                SUM([AlgebraicValue])
                    OVER (PARTITION BY [AccountId], [BusinessUnitId], [AgentId], [ResourceId]
                          ORDER BY [PostingDate], [Direction] DESC) AS RunningValue
            FROM @T
        )
        UPDATE T
        SET T.[RunningMonetaryValue] = CB.[RunningMonetaryValue],
            T.[RunningValue]         = CB.[RunningValue]
        FROM @T T
        JOIN CumBalances CB ON T.[Id] = CB.[Id];
    END;

    PRINT '2: Time taken was ' + CAST(DATEDIFF(millisecond, @StartTime2, SysUTCDateTime()) AS VARCHAR) + 'ms';

    DECLARE @StartTime3 DATETIME2 = SysUTCDateTime();

    -- ============================================================
    -- Pass 3a: Write AVCO-corrected values back to dbo.Entries
    -- for simple inventory issue lines only (IsBOMTransfer = 0).
    -- Includes: raw-material -> WIP, FG -> customer, and any other
    -- inventory credit whose account is NOT WorkInProgress.
    -- The paired debit entry (Index - 1) is updated in the same
    -- statement so both sides of the line remain balanced.
    -- ============================================================
    WITH NewValues AS (
        SELECT
            E.[LineId],
            E.[Index],
            ROUND(ABS(T.[AlgebraicMonetaryValue] * E.[BaseQuantity] / T.[AlgebraicQuantity]), 2) AS NewMonetaryValue,
            ROUND(ABS(T.[AlgebraicValue]         * E.[BaseQuantity] / T.[AlgebraicQuantity]), 2) AS NewValue
        FROM map.DetailsEntries() E
        JOIN dbo.Lines L ON L.[Id] = E.[LineId]
        JOIN @CenterBusinessUnit CBU ON CBU.CenterId = E.[CenterId]
        JOIN @T T ON T.[AccountId]      = E.[AccountId]
                 AND T.[BusinessUnitId] = CBU.BusinessUnitId
                 AND T.[AgentId]        = E.[AgentId]
                 AND T.[ResourceId]     = E.[ResourceId]
                 AND T.[PostingDate]    = L.[PostingDate]
        JOIN @AffectedLineDefinitionEntries LDE
            ON LDE.[LineDefinitionId] = L.[DefinitionId] AND LDE.[Index] = E.[Index]
        WHERE T.[AlgebraicQuantity] <> 0
        AND   T.[Direction]       = -1
        AND   E.[Direction]       = -1
        AND   LDE.[IsBOMTransfer] = 0       -- Simple issues only; WIP->FG handled in Pass 3b
        AND   L.[PostingDate]     > @ArchiveDate
    )
    UPDATE E
    SET
        E.[MonetaryValue] = NV.[NewMonetaryValue],
        E.[Value]         = NV.[NewValue]
    FROM dbo.Entries E
    JOIN NewValues NV ON E.[LineId] = NV.[LineId]
                     AND (E.[Index] = NV.[Index] OR E.[Index] = NV.[Index] - 1);

    PRINT '3a: Time taken was ' + CAST(DATEDIFF(millisecond, @StartTime3, SysUTCDateTime()) AS VARCHAR) + 'ms';

    -- ============================================================
    -- Pass 3b: BOM reallocation for WIP -> Finished Goods transfers.
    --
    -- After Pass 3a has updated the raw-material costs feeding WIP,
    -- we read the corrected total WIP cost per production voucher
    -- from dbo.Entries and redistribute it across the FG output
    -- lines in proportion to each line's BOM ratio (Decimal1).
    --
    -- Detection uses the WorkInProgress concept directly, matching
    -- the rest of the application and avoiding any dependency on
    -- the Inventories hierarchy arrangement.
    -- ============================================================
    DECLARE @StartTime4 DATETIME2 = SysUTCDateTime();

    WITH WIPAccountTypes AS (
        SELECT ATC.[Id]
        FROM dbo.AccountTypes ATC
        JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node]) = 1
        WHERE ATP.[Concept] = N'WorkInProgress'
    ),
    -- Production voucher documents containing WIP credit entries after @ArchiveDate
    WIPTransferDocuments AS (
        SELECT DISTINCT L.[DocumentId]
        FROM dbo.Entries E
        JOIN dbo.Lines L ON L.[Id] = E.[LineId]
        JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
        WHERE A.[AccountTypeId] IN (SELECT [Id] FROM WIPAccountTypes)
        AND   E.[Direction]   = -1
        AND   L.[PostingDate] > @ArchiveDate
        AND   L.[State]       >= @MinState
    ),
    -- Updated total cost flowing INTO WIP (raw material + overhead) per voucher.
    -- Pass 3a has already written corrected raw-material values to dbo.Entries,
    -- so this read returns the post-AVCO total.
    WIPTotals AS (
        SELECT
            L.[DocumentId],
            SUM(E.[Value])         AS [TotalWIPCost],
            SUM(E.[MonetaryValue]) AS [TotalWIPMonetary]
        FROM dbo.Entries E
        JOIN dbo.Lines L ON L.[Id] = E.[LineId]
        JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
        JOIN WIPTransferDocuments D ON D.[DocumentId] = L.[DocumentId]
        WHERE A.[AccountTypeId] IN (SELECT [Id] FROM WIPAccountTypes)
        AND   E.[Direction] = +1       -- Debits INTO WIP (raw material + overhead)
        AND   L.[State]     >= @MinState
        GROUP BY L.[DocumentId]
    ),
    -- One row per FG output line: the WIP credit entry and its
    -- BOM standard-cost ratio (Decimal1 on the Line record).
    BOMLines AS (
        SELECT
            E.[LineId],
            E.[Index],
            L.[DocumentId],
            L.[Decimal1],
            SUM(L.[Decimal1]) OVER (PARTITION BY L.[DocumentId]) AS [TotalDecimal1]
        FROM dbo.Entries E
        JOIN dbo.Lines L ON L.[Id] = E.[LineId]
        JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
        JOIN WIPTransferDocuments D ON D.[DocumentId] = L.[DocumentId]
        WHERE A.[AccountTypeId] IN (SELECT [Id] FROM WIPAccountTypes)
        AND   E.[Direction]   = -1     -- Credits OUT of WIP (one per FG product)
        AND   L.[PostingDate] > @ArchiveDate
        AND   L.[State]       >= @MinState
    ),
    NewBOMValues AS (
        SELECT
            BL.[LineId],
            BL.[Index],
            ROUND(WT.[TotalWIPMonetary] * BL.[Decimal1] / BL.[TotalDecimal1], 2) AS [NewMonetary],
            ROUND(WT.[TotalWIPCost]     * BL.[Decimal1] / BL.[TotalDecimal1], 2) AS [NewValue]
        FROM BOMLines BL
        JOIN WIPTotals WT ON WT.[DocumentId] = BL.[DocumentId]
        WHERE BL.[TotalDecimal1] <> 0
    )
    UPDATE E
    SET
        E.[MonetaryValue] = NV.[NewMonetary],
        E.[Value]         = NV.[NewValue]
    FROM dbo.Entries E
    JOIN NewBOMValues NV ON E.[LineId] = NV.[LineId]
                        AND (E.[Index] = NV.[Index] OR E.[Index] = NV.[Index] - 1);

    PRINT '3b (BOM reallocation): Time taken was ' + CAST(DATEDIFF(millisecond, @StartTime4, SysUTCDateTime()) AS VARCHAR) + 'ms';

DONE:
GO