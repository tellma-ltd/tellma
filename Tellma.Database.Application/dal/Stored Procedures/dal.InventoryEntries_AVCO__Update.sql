CREATE PROCEDURE [dal].[InventoryEntries_AVCO__Update]
-- [dal].[InventoryEntries_AVCO__Update] @ArchiveDate = N'2025.08.01', @MinState = 0;
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
    -- WIP accounts, resolved once and reused throughout.
    -- ============================================================
    DECLARE @WIPAccountIds TABLE ([Id] INT PRIMARY KEY);
    INSERT INTO @WIPAccountIds ([Id])
    SELECT A.[Id]
    FROM dbo.Accounts A
    WHERE A.[AccountTypeId] IN (
        SELECT ATC.[Id]
        FROM dbo.AccountTypes ATC
        JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node]) = 1
        WHERE ATP.[Concept] = N'WorkInProgress'
    );

    -- ============================================================
    -- @BOMDocuments: DOCUMENT-LEVEL classification.
    --
    -- A document is a true "production voucher" (BOM allocation
    -- applies) ONLY when it contains BOTH:
    --   * WIP debit entries  (inputs:  raw material + overhead), AND
    --   * WIP credit entries (outputs: finished goods).
    -- Example: the PV documents in tenant 110.
    --
    -- A WIP credit in a document with NO WIP debits (e.g. an SRV
    -- "WIP => Output" receipt voucher, where the inputs were posted
    -- by separate SIV / ECV documents) is NOT a BOM allocation.
    -- It is an ordinary issue out of the WIP pool, and the AVCO
    -- loop must price it at the WIP running average cost, which
    -- naturally absorbs the overhead debits posted to the same
    -- WIP partition.
    --
    -- The line definitions are structurally identical in both
    -- workflows (credit WIP, debit FG), so the distinction can
    -- only be made at the document level, not the line definition
    -- level.
    -- ============================================================
    DECLARE @BOMDocuments TABLE ([DocumentId] INT PRIMARY KEY);
    INSERT INTO @BOMDocuments ([DocumentId])
    SELECT L.[DocumentId]
    FROM dbo.Entries E
    JOIN dbo.Lines L ON L.[Id] = E.[LineId]
    WHERE E.[AccountId] IN (SELECT [Id] FROM @WIPAccountIds)
    AND   L.[State] >= @MinState
    GROUP BY L.[DocumentId]
    HAVING MIN(E.[Direction]) = -1  -- has WIP credits (outputs)
       AND MAX(E.[Direction]) = +1; -- has WIP debits  (inputs)

    -- ============================================================
    -- @AffectedLineDefinitionEntries
    -- IsBOMTransfer = 1 marks line definitions whose credit side is
    -- a WorkInProgress account.  This is only a CANDIDATE flag:
    -- an entry is treated as a BOM transfer at runtime only when
    -- its document is also in @BOMDocuments.
    -- ============================================================
    DECLARE @AffectedLineDefinitionEntries TABLE (
        [LineDefinitionId]  INT,
        [Index]             INT,
        [IsBOMTransfer]     BIT NOT NULL DEFAULT 0,
        PRIMARY KEY ([LineDefinitionId], [Index])
    );

    -- ============================================================
    -- @T carries IsBOMTransfer (already combined with the document-
    -- level test) so the AVCO loop can filter with a plain WHERE.
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
    -- Identify affected line definitions and set the candidate flag.
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
    -- Line definitions that credit a WIP account specifically
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
    -- Only check simple issue lines (candidate flag = 0).
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
    --
    -- An aggregated row is flagged IsBOMTransfer = 1 ONLY when it
    -- contains an entry that is BOTH:
    --   * on a WIP-credit line definition (candidate flag), AND
    --   * inside a document classified in @BOMDocuments.
    -- WIP credits in single-sided documents (SRV pattern) therefore
    -- get IsBOMTransfer = 0 and flow through the AVCO loop as
    -- ordinary issues out of the WIP pool.
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
            CAST(MAX(CASE WHEN LDE.[IsBOMTransfer] = 1 AND BD.[DocumentId] IS NOT NULL
                          THEN 1 ELSE 0 END) AS BIT)  AS [IsBOMTransfer],
            ISNULL(SUM(E.[Direction] * E.[BaseQuantity]),  0) AS [AlgebraicQuantity],
            SUM(E.[Direction] * E.[MonetaryValue])           AS [AlgebraicMonetaryValue],
            SUM(E.[Direction] * E.[Value])                   AS [AlgebraicValue]
        FROM map.DetailsEntries() E
        JOIN dbo.Lines L ON L.[Id] = E.[LineId]
        JOIN @CenterBusinessUnit CBU ON CBU.CenterId = E.[CenterId]
        LEFT JOIN @AffectedLineDefinitionEntries LDE
            ON  LDE.[LineDefinitionId] = L.[DefinitionId]
            AND LDE.[Index]            = E.[Index]
        LEFT JOIN @BOMDocuments BD
            ON  BD.[DocumentId]        = L.[DocumentId]
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
    -- Processes all ordinary issues (IsBOMTransfer = 0), which now
    -- includes WIP => Output credits from single-sided documents.
    -- Only same-document production voucher outputs are excluded
    -- (they are allocated in Pass 3b instead).
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
        AND   T.[IsBOMTransfer] = 0    -- Exclude only same-document production voucher outputs
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

        -- Update ordinary issues within the batch window.
        -- AccountId added to both joins, and BOM rows explicitly
        -- excluded, so a batch can never clobber rows outside its
        -- own account or production voucher outputs that happen to
        -- share (BusinessUnit, Agent, Resource).
        UPDATE T
        SET
            T.[AlgebraicMonetaryValue] = T.[AlgebraicQuantity] * BS.[MVPU],
            T.[AlgebraicValue]         = T.[AlgebraicQuantity] * BS.[VPU]
        FROM @T T
        JOIN @BatchStartAndVPU BS ON T.[AccountId]       = BS.[AccountId]
                                 AND T.[AgentId]         = BS.[AgentId]
                                 AND T.[ResourceId]      = BS.[ResourceId]
                                 AND T.[BusinessUnitId]  = BS.[BusinessUnitId]
        LEFT JOIN @BatchEnd    BE ON T.[AccountId]       = BE.[AccountId]
                                 AND T.[AgentId]         = BE.[AgentId]
                                 AND T.[ResourceId]      = BE.[ResourceId]
                                 AND T.[BusinessUnitId]  = BE.[BusinessUnitId]
        WHERE T.[PostingDate] >= BS.[PostingDate]
        AND  (BE.[PostingDate] IS NULL OR T.[PostingDate] < BE.[PostingDate])
        AND   ABS(T.[AlgebraicValue] - T.[PriorVPU] * T.[AlgebraicQuantity]) > @Epsilon
        AND   T.[Direction]     = -1
        AND   T.[IsBOMTransfer] = 0;

        -- Convergence exit: must come IMMEDIATELY after the UPDATE
        -- so @@ROWCOUNT still refers to it.  Without this line the
        -- loop always runs to the iteration limit.
        IF @@ROWCOUNT = 0 BREAK;

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
    -- Pass 3a: Write AVCO-corrected values back to dbo.Entries for
    -- every ordinary issue — including WIP => Output credits from
    -- single-sided documents (SRV pattern), which are excluded only
    -- when their document is a true production voucher.
    -- The paired debit entry (Index - 1) is updated in the same
    -- statement so both sides of the line remain balanced; for an
    -- SRV this is what carries the WIP average cost onto the
    -- finished goods receipt side.
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
        LEFT JOIN @BOMDocuments BD
            ON BD.[DocumentId] = L.[DocumentId]
        WHERE T.[AlgebraicQuantity] <> 0
        AND   T.[Direction]       = -1
        AND   E.[Direction]       = -1
        -- Exclude only entries that are BOTH on a WIP-credit line
        -- definition AND inside a production voucher document:
        AND  (LDE.[IsBOMTransfer] = 0 OR BD.[DocumentId] IS NULL)
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
    -- Pass 3b: BOM reallocation — production voucher documents ONLY
    -- (documents containing both WIP debits and WIP credits).
    --
    -- After Pass 3a has updated the raw-material costs feeding WIP,
    -- the total WIP debit per production voucher is read back from
    -- dbo.Entries and redistributed across the document's FG output
    -- lines in proportion to each line's BOM ratio (Decimal1).
    -- ============================================================
    DECLARE @StartTime4 DATETIME2 = SysUTCDateTime();

    WITH WIPTotals AS (
        SELECT
            L.[DocumentId],
            SUM(E.[Value])         AS [TotalWIPCost],
            SUM(E.[MonetaryValue]) AS [TotalWIPMonetary]
        FROM dbo.Entries E
        JOIN dbo.Lines L ON L.[Id] = E.[LineId]
        JOIN @BOMDocuments D ON D.[DocumentId] = L.[DocumentId]
        WHERE E.[AccountId] IN (SELECT [Id] FROM @WIPAccountIds)
        AND   E.[Direction] = +1       -- Debits INTO WIP (raw material + overhead)
        AND   L.[State]     >= @MinState
        GROUP BY L.[DocumentId]
    ),
    BOMLines AS (
        SELECT
            E.[LineId],
            E.[Index],
            L.[DocumentId],
            L.[Decimal1],
            SUM(L.[Decimal1]) OVER (PARTITION BY L.[DocumentId]) AS [TotalDecimal1]
        FROM dbo.Entries E
        JOIN dbo.Lines L ON L.[Id] = E.[LineId]
        JOIN @BOMDocuments D ON D.[DocumentId] = L.[DocumentId]
        WHERE E.[AccountId] IN (SELECT [Id] FROM @WIPAccountIds)
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
        AND   BL.[Decimal1] IS NOT NULL   -- never write NULL into Entries
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