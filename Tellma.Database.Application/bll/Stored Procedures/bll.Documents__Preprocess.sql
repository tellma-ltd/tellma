CREATE PROCEDURE [bll].[Documents__Preprocess]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].[EntryList] READONLY,
	@PreprocessedEntriesJson NVARCHAR (MAX) = NULL OUTPUT 
AS
BEGIN
	--=-=-=-=-=-=- [C# Preprocessing before SQL]
	/* 
	
	 [✓] If Clearance is NULL, set it to 0
	 [✓] If a line has the wrong number of entries, fix it
	 [✓] Set all Entries' Directions according to definition (except for manual lines)
	 [✓] Copy all IsCommon values from the documents to the lines and entries

	*/

	SET NOCOUNT ON;
	DECLARE @FunctionalCurrencyId NCHAR(3) = dbo.fn_FunctionalCurrencyId();
	DECLARE @ScriptWideLines dbo.WideLineList, @ScriptLineDefinitions dbo.StringList, @LineDefinitionId INT;
	DECLARE @WL dbo.[WideLineList], @PreprocessedWideLines dbo.[WideLineList];
	DECLARE @ScriptLines dbo.LineList, @ScriptEntries dbo.EntryList;
	DECLARE @PreprocessedDocuments [dbo].[DocumentList], @PreprocessedLines [dbo].[LineList], @PreprocessedEntries [dbo].[EntryList];
	DECLARE @D [dbo].[DocumentList], @L [dbo].[LineList], @E [dbo].[EntryList];
	DECLARE @Today DATE = CAST(GETDATE() AS DATE);
	DECLARE @ManualLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
	DECLARE @ExchangeVarianceLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ExchangeVariance');
	DECLARE @CostReallocationToInvestmentPropertyUnderConstructionOrDevelopmentLD INT = 
		(SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CostReallocationToInvestmentPropertyUnderConstructionOrDevelopment');

	DECLARE @PreScript NVARCHAR(MAX) =N'
	SET NOCOUNT ON
	DECLARE @ProcessedWideLines WideLineList;

	INSERT INTO @ProcessedWideLines
	SELECT * FROM @WideLines;
	------
	';
	DECLARE @Script NVARCHAR (MAX);
	DECLARE @PostScript NVARCHAR(MAX) = N'
	-----
	SELECT * FROM @ProcessedWideLines;
	';
	INSERT INTO @D SELECT * FROM @Documents;
	INSERT INTO @L SELECT * FROM @Lines;
	INSERT INTO @E SELECT * FROM @Entries;

	IF (SELECT COUNT(*) FROM dbo.Centers WHERE [IsSegment] = 1 AND [IsActive] = 1) = 1
	BEGIN
		DECLARE @SegmentId INT = (SELECT [Id] FROM dbo.Centers WHERE [IsSegment] = 1 AND[IsActive] = 1);
		UPDATE @D SET [SegmentId] = @SegmentId
	END
--	Remove Residuals
	UPDATE E
	SET E.[ResourceId] = NULL, E.Quantity = NULL, E.UnitId = NULL
	FROM @E E
	JOIN @L L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	WHERE  A.ResourceDefinitionId IS NULL;

	UPDATE E
	SET E.[CustodyId] = NULL
	FROM @E E
	JOIN @L L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	WHERE A.[CustodyDefinitionId] IS NULL;

	UPDATE E
	SET E.EntryTypeId = NULL
	FROM @E E
	JOIN @L L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.Id
	WHERE AC.EntryTypeParentId IS NULL;

	UPDATE E
	SET E.[NotedRelationId] = NULL
	FROM @E E
	JOIN @L L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.[Id]
	WHERE AC.NotedRelationDefinitionId IS NULL;

	-- TODO:  Remove labels, etc.

--	Overwrite input with data specified in the template (or clause)
	UPDATE E
	SET
		E.[Direction]		= COALESCE(ES.[Direction], E.[Direction]),
		E.[AccountId]		= COALESCE(ES.[AccountId], E.[AccountId]),
		E.[CurrencyId]		= COALESCE(ES.[CurrencyId], E.[CurrencyId]),
		E.[CustodyId]		= COALESCE(ES.[CustodyId], E.[CustodyId]),
		E.[ResourceId]		= COALESCE(ES.[ResourceId], E.[ResourceId]),
		E.[CenterId]		= COALESCE(ES.[CenterId], E.[CenterId]),
		E.[EntryTypeId]		= COALESCE(ES.[EntryTypeId], E.[EntryTypeId]),
		E.[MonetaryValue]	= COALESCE(L.[Multiplier] * ES.[MonetaryValue], E.[MonetaryValue]),
		E.[Quantity]		= COALESCE(L.[Multiplier] * ES.[Quantity], E.[Quantity]),
		E.[UnitId]			= COALESCE(ES.[UnitId], E.[UnitId]),
--		E.[Value]			= COALESCE(L.[Multiplier] * ES.[Value], E.[Value]),
		E.[Time1]			= COALESCE(ES.[Time1], E.[Time1]),
		E.[Time2]			= COALESCE(ES.[Time2], E.[Time2]),
		E.[ExternalReference]= COALESCE(ES.[ExternalReference], E.[ExternalReference]),
		E.[AdditionalReference]= COALESCE(ES.[AdditionalReference], E.[AdditionalReference]),
		E.[NotedRelationId]	= COALESCE(ES.[NotedRelationId], E.[NotedRelationId]),
		E.[NotedAgentName]	= COALESCE(ES.[NotedAgentName], E.[NotedAgentName]),
		E.[NotedAmount]		= COALESCE(ES.[NotedAmount], E.[NotedAmount]),
		E.[NotedDate]		= COALESCE(ES.[NotedDate], E.[NotedDate])
	FROM @E E
	JOIN @L L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Lines LS ON L.[TemplateLineId] = LS.[Id]
	JOIN dbo.Entries ES ON ES.[LineId] = LS.[Id]
	WHERE E.[Index] = ES.[Index];
	-- Overwrite input with DB data that is read only
	-- TODO : Overwrite readonly Memo
	WITH CTE AS (
		SELECT
			E.[Index], E.[LineIndex], E.[DocumentIndex], E.[CurrencyId], E.[CenterId], E.[CustodyId],
			E.[ResourceId], E.[Quantity], E.[UnitId], E.[MonetaryValue], E.[Time1], E.[Time2],  E.[ExternalReference], 
			E.[AdditionalReference], E.[NotedRelationId],  E.[NotedAgentName],  E.[NotedAmount],  E.[NotedDate], 
			E.[EntryTypeId], LDC.[ColumnName]
		FROM @E E
		JOIN dbo.Entries BE ON E.Id = BE.Id
		JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
		JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
		WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	)
	UPDATE E
	SET
		E.[CurrencyId]			= IIF(CTE.[ColumnName] = N'CurrencyId', CTE.[CurrencyId], E.[CurrencyId]),
		E.[CenterId]			= IIF(CTE.[ColumnName] = N'CenterId', CTE.[CenterId], E.[CenterId]),
		E.[CustodyId]			= IIF(CTE.[ColumnName] = N'CustodyId', CTE.[CustodyId], E.[CustodyId]),
		E.[ResourceId]			= IIF(CTE.[ColumnName] = N'ResourceId', CTE.[ResourceId], E.[ResourceId]),
		E.[Quantity]			= IIF(CTE.[ColumnName] = N'Quantity', CTE.[Quantity], E.[Quantity]),
		E.[UnitId]				= IIF(CTE.[ColumnName] = N'UnitId', CTE.[UnitId], E.[UnitId]),
		E.[MonetaryValue]		= IIF(CTE.[ColumnName] = N'MonetaryValue', CTE.[MonetaryValue], E.[MonetaryValue]),
		E.[Time1]				= IIF(CTE.[ColumnName] = N'Time1', CTE.[Time1], E.[Time1]),
		E.[Time2]				= IIF(CTE.[ColumnName] = N'Time2', CTE.[Time2], E.[Time2]),
		E.[ExternalReference]	= IIF(CTE.[ColumnName] = N'ExternalReference', CTE.[ExternalReference], E.[ExternalReference]),
		E.[AdditionalReference]	= IIF(CTE.[ColumnName] = N'AdditionalReference', CTE.[AdditionalReference], E.[AdditionalReference]),
		E.[NotedRelationId]		= IIF(CTE.[ColumnName] = N'NotedRelationId', CTE.[NotedRelationId], E.[NotedRelationId]),
		E.[NotedAgentName]		= IIF(CTE.[ColumnName] = N'NotedAgentName', CTE.[NotedAgentName], E.[NotedAgentName]),
		E.[NotedAmount]			= IIF(CTE.[ColumnName] = N'NotedAmount', CTE.[NotedAmount], E.[NotedAmount]),
		E.[NotedDate]			= IIF(CTE.[ColumnName] = N'NotedDate', CTE.[NotedDate], E.[NotedDate]),
		E.[EntryTypeId]			= IIF(CTE.[ColumnName] = N'EntryTypeId', CTE.[EntryTypeId], E.[EntryTypeId])
	FROM @E E
	JOIN CTE ON  E.[Index] = CTE.[Index] AND E.[LineIndex] = CTE.[LineIndex] AND E.[DocumentIndex] = CTE.[DocumentIndex];

	-- Get line definition which have script to run
	INSERT INTO @ScriptLineDefinitions
	SELECT DISTINCT DefinitionId FROM @L
	WHERE DefinitionId IN (
		SELECT [Id] FROM dbo.LineDefinitions
		WHERE [PreprocessScript] IS NOT NULL
	);
	-- Copy lines and entries with no script as they are
	INSERT INTO @PreprocessedDocuments
	SELECT * FROM @D
	INSERT INTO @PreprocessedLines
	SELECT * FROM @L WHERE DefinitionId NOT IN (SELECT [Id] FROM @ScriptLineDefinitions)
	INSERT INTO @PreprocessedEntries
	SELECT E.*
	FROM @E E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	-- Populate PreprocessedLines and PreprocessedEntries using script
	IF EXISTS (SELECT * FROM @ScriptLineDefinitions)
	BEGIN
		INSERT INTO @ScriptLines SELECT * FROM @L WHERE DefinitionId IN (SELECT [Id] FROM @ScriptLineDefinitions)
		INSERT INTO @ScriptEntries
		SELECT E.* FROM @E E
		JOIN @ScriptLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
		-- Flatten lines/entries
		INSERT INTO @ScriptWideLines--** causes nested INSERT EXEC
		EXEC [bll].[Lines__Pivot] @ScriptLines, @ScriptEntries;
		-- run script to fill missing information
		DECLARE LineDefinition_Cursor CURSOR FOR SELECT [Id] FROM @ScriptLineDefinitions;  
		OPEN LineDefinition_Cursor  
		FETCH NEXT FROM LineDefinition_Cursor INTO @LineDefinitionId; 
		WHILE @@FETCH_STATUS = 0  
		BEGIN 
			SELECT @Script = @PreScript + ISNULL([PreprocessScript],N'') + @PostScript
			FROM dbo.LineDefinitions WHERE [Id] = @LineDefinitionId;

			DELETE FROM @WL;
			INSERT INTO @WL SELECT * FROM @ScriptWideLines WHERE [DefinitionId] = @LineDefinitionId;

			INSERT INTO @PreprocessedWideLines--** causes nested INSERT EXEC
			EXECUTE	sp_executesql @Script, N'@WideLines WideLineList READONLY', @WideLines = @WL;
			
			FETCH NEXT FROM LineDefinition_Cursor INTO @LineDefinitionId;
		END
		INSERT INTO @PreprocessedLines SELECT * FROM @ScriptLines;
		INSERT INTO @PreprocessedEntries	
		EXEC bll.WideLines__Unpivot @PreprocessedWideLines
	END
	-- for all lines, Get currency and center from Resources
	DECLARE @BalanceSheetNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'StatementOfFinancialPositionAbstract');

	WITH BalanceSheetAccounts AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [AccountTypeId] IN (
			SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@BalanceSheetNode) = 1
		)
	)
	-- This works for JVs only, since in intelligent screens, the account is null
	UPDATE E 
	SET
		E.[CenterId]		= COALESCE(R.[CenterId], E.[CenterId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.[Resources] R ON E.ResourceId = R.Id
	JOIN BalanceSheetAccounts A ON E.[AccountId] = A.[Id]

	UPDATE E
	SET
		E.[CenterId]		= COALESCE(R.[CenterId], E.[CenterId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND LDE.[Index] = E.[Index]
	JOIN dbo.AccountTypes AC ON LDE.[AccountTypeId] = AC.[Id]
	JOIN dbo.[Resources] R ON E.[ResourceId] = R.[Id]
	 WHERE AC.[Node].IsDescendantOf(@BalanceSheetNode) = 1

	UPDATE E 
	SET
		E.[CurrencyId]		= R.[CurrencyId],
		E.[MonetaryValue]	= COALESCE(R.[MonetaryValue], E.[MonetaryValue])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.[Resources] R ON E.ResourceId = R.Id;

	-- for all lines, Get currency and center from Custodies if available.
	UPDATE E 
	SET
		E.[CenterId]		= COALESCE(C.[CenterId], E.[CenterId]),
		E.[CurrencyId]		= COALESCE(C.[CurrencyId], E.[CurrencyId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.[Custodies] C ON E.[CustodyId] = C.Id;
	-- When the resource has exactly one non-null unit Id, and the account does not allow PureUnit set it as the Entry's UnitId
	UPDATE E
	SET E.[UnitId] = COALESCE(R.UnitId, E.[UnitId])
	FROM @PreprocessedEntries E
	JOIN dbo.[Resources] R ON E.ResourceId = R.Id
	JOIN dbo.ResourceDefinitions RD ON R.[DefinitionId] = RD.[Id]
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
	WHERE
		RD.UnitCardinality IN (N'Single', N'None')
	AND AC.[AllowsPureUnit] = 0

	UPDATE E
	SET E.[Quantity] = 1
	FROM @PreprocessedEntries E
	JOIN dbo.Units U ON E.[UnitId] = U.[Id]
	WHERE U.UnitType = N'Pure'

	-- Copy information from Account to entries
	UPDATE E 
	SET
		E.[CurrencyId]		= COALESCE(A.[CurrencyId], E.[CurrencyId]),
		E.[CustodyId]		= COALESCE(A.[CustodyId], E.[CustodyId]),
		E.[ResourceId]		= COALESCE(A.[ResourceId], E.[ResourceId]),
		E.[CenterId]		= COALESCE(A.[CenterId], E.[CenterId]),
		E.[EntryTypeId]		= COALESCE(A.[EntryTypeId], E.[EntryTypeId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	WHERE L.DefinitionId = @ManualLineLD;

	-- Copy information from Line definitions to Entries
	UPDATE E
	SET
	--	E.[Direction] = LDE.[Direction], -- Handled in C#
		E.[EntryTypeId] = COALESCE(LDE.[EntryTypeId], E.[EntryTypeId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.LineDefinitionEntries LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[Index] = LDE.[Index]
	WHERE L.[DefinitionId] <> @ManualLineLD;

	-- For financial amounts in foreign currency, the rate is manually entered or read from a web service
	UPDATE E 
	SET E.[Value] = ROUND(ER.[Rate] * E.[MonetaryValue], C.[E])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [map].[ExchangeRates]() ER ON E.CurrencyId = ER.CurrencyId
	JOIN dbo.Currencies C ON E.CurrencyId = C.[Id]
	WHERE
		ER.ValidAsOf <= ISNULL(L.[PostingDate], @Today)
	AND ER.ValidTill >	ISNULL(L.[PostingDate], @Today)
	AND L.[DefinitionId] <> @ManualLineLD
	AND L.[DefinitionId] IN (SELECT [Id] FROM dbo.LineDefinitions WHERE [GenerateScript] IS NULL);

	-- Set the Account based on provided info so far
	With LineEntries AS (
		SELECT E.[Index], E.[LineIndex], E.[DocumentIndex], ATC.[Id] AS [AccountTypeId], R.[DefinitionId] AS ResourceDefinitionId, E.[ResourceId],
				C.[DefinitionId] AS [CustodyDefinitionId], E.[CustodyId], E.[CenterId], E.[CurrencyId]
		FROM @PreprocessedEntries E
		JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
		JOIN dbo.[LineDefinitionEntries] LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[Index] = LDE.[Index]
		JOIN dbo.AccountTypes ATP ON LDE.[AccountTypeId] = ATP.[Id]
		JOIN dbo.AccountTypes ATC ON ATC.[Node].IsDescendantOf(ATP.[Node]) = 1
		LEFT JOIN dbo.[Resources] R ON E.[ResourceId] = R.[Id]
		LEFT JOIN dbo.[Custodies] C ON E.[CustodyId] = C.[Id]
		WHERE L.DefinitionId <> @ManualLineLD
		AND ATC.[IsActive] = 1 AND ATC.[IsAssignable] = 1
		--WHERE (R.[DefinitionId] IS NULL OR R.[DefinitionId] IN (
		--	SELECT [ResourceDefinitionId] FROM [LineDefinitionEntryResourceDefinitions]
		--	WHERE [LineDefinitionEntryId] = LDE.[Id]
		--))
		--AND (C.[DefinitionId] IS NULL OR C.[DefinitionId] IN (
		--	SELECT [CustodyDefinitionId] FROM [LineDefinitionEntryCustodyDefinitions]
		--	WHERE [LineDefinitionEntryId] = LDE.[Id]		
		--))
	),
	ConformantAccounts AS (
		SELECT LE.[Index], LE.[LineIndex], LE.[DocumentIndex], MIN(A.Id) AS MINAccountId, MAX(A.[Id]) AS MAXAccountId
		FROM dbo.Accounts A
		JOIN LineEntries LE ON LE.[AccountTypeId] = A.[AccountTypeId]
		WHERE
			(A.[IsActive] = 1)
		AND	(A.[CenterId] IS NULL OR A.[CenterId] = LE.[CenterId])
		AND (A.[CurrencyId] IS NULL OR A.[CurrencyId] = LE.[CurrencyId])
		AND (A.[ResourceDefinitionId] IS NULL AND LE.[ResourceDefinitionId] IS NULL OR A.[ResourceDefinitionId] = LE.[ResourceDefinitionId])
		AND (A.[ResourceId] IS NULL OR A.[ResourceId] = LE.[ResourceId])
		AND (A.[CustodyDefinitionId] IS NULL AND LE.[CustodyDefinitionId] IS NULL OR A.[CustodyDefinitionId] = LE.[CustodyDefinitionId])
		AND (A.[CustodyId] IS NULL OR A.[CustodyId] = LE.[CustodyId])
		GROUP BY LE.[Index], LE.[LineIndex], LE.[DocumentIndex]
	)
	UPDATE E -- Override the Account when there is exactly one solution. Otherwise, leave it.
	SET E.AccountId = CA.MINAccountId
	FROM @PreprocessedEntries E
	JOIN ConformantAccounts CA ON E.[Index] = CA.[Index] AND E.[LineIndex] = CA.[LineIndex] AND E.[DocumentIndex] = CA.[DocumentIndex]
	WHERE CA.MINAccountId = CA.MAXAccountId;

	With LineEntries2 AS (
		SELECT E.[Index], E.[LineIndex], E.[DocumentIndex], ATC.[Id] AS [AccountTypeId], R.[DefinitionId] AS ResourceDefinitionId, E.[ResourceId],
				C.[DefinitionId] AS [CustodyDefinitionId], E.[CustodyId], E.[CenterId], E.[CurrencyId]
		FROM @PreprocessedEntries E
		JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
		JOIN dbo.[LineDefinitionEntries] LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[Index] = LDE.[Index]
		JOIN dbo.AccountTypes ATP ON LDE.[AccountTypeId] = ATP.[Id]
		JOIN dbo.AccountTypes ATC ON ATC.[Node].IsDescendantOf(ATP.[Node]) = 1
		LEFT JOIN dbo.[Resources] R ON E.[ResourceId] = R.[Id]
		LEFT JOIN dbo.[Custodies] C ON E.[CustodyId] = C.[Id]
		WHERE L.DefinitionId <> @ManualLineLD
		AND ATC.[IsActive] = 1 AND ATC.[IsAssignable] = 1
	),
	ConformantAccounts2 AS (
		SELECT LE.[Index], LE.[LineIndex], LE.[DocumentIndex], A.[Id] AS AccountId
		FROM dbo.Accounts A
		JOIN LineEntries2 LE ON LE.[AccountTypeId] = A.[AccountTypeId]
		WHERE
			(A.[IsActive] = 1)
		AND	(A.[CenterId] IS NULL OR A.[CenterId] = LE.[CenterId])
		AND (A.[CurrencyId] IS NULL OR A.[CurrencyId] = LE.[CurrencyId])
		AND (A.[ResourceDefinitionId] IS NULL AND LE.[ResourceDefinitionId] IS NULL OR A.[ResourceDefinitionId] = LE.[ResourceDefinitionId])
		AND (A.[ResourceId] IS NULL OR A.[ResourceId] = LE.[ResourceId])
		AND (A.[CustodyDefinitionId] IS NULL AND LE.[CustodyDefinitionId] IS NULL OR A.[CustodyDefinitionId] = LE.[CustodyDefinitionId])
		AND (A.[CustodyId] IS NULL OR A.[CustodyId] = LE.[CustodyId])
	)
	UPDATE E -- Set account to null, if non conformant
	SET E.AccountId = NULL
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	LEFT JOIN ConformantAccounts2 CA
	ON E.[Index] = CA.[Index] AND E.[LineIndex] = CA.[LineIndex] AND E.[DocumentIndex] = CA.[DocumentIndex] AND E.AccountId = CA.AccountId
	WHERE L.DefinitionId  <> @ManualLineLD
	AND E.AccountId IS NOT NULL AND CA.AccountId IS NULL;

	-- Return the populated entries.
	-- (Later we may need to return the populated lines and documents as well)
	SELECT @PreprocessedEntriesJson = 
	(
		SELECT *
		FROM @PreprocessedEntries
		FOR JSON PATH
	);	

	-- We're still assuming that preprocess only modifies, it doesn't insert nor deletes
	SELECT * FROM @PreprocessedDocuments;
	SELECT * FROM @PreprocessedLines;
	SELECT * FROM @PreprocessedEntries;
END

	--=-=-=-=-=-=- [C# Preprocessing after SQL]
	/* 
	
	 [✓] For Smart Lines: If CurrencyId == functional set Value = MonetaryValue
	 [✓] For Manual Lines: If CurrencyId == functional set MonetaryValue = Value

	*/