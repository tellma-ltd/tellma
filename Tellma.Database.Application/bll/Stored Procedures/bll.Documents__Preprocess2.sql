CREATE PROCEDURE [bll].[Documents__Preprocess2]
	@DefinitionId NVARCHAR(50),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].EntryList READONLY,
	@PreprocessedEntriesJson NVARCHAR (MAX) = NULL OUTPUT 
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @FunctionalCurrencyId NCHAR(3) = CONVERT(NCHAR(3), SESSION_CONTEXT(N'FunctionalCurrencyId'));
	DECLARE @ScriptWideLines dbo.WideLineList, @ScriptLineDefinitions dbo.StringList, @LineDefinitionId NVARCHAR(50);
	DECLARE @WL dbo.[WideLineList], @PreprocessedWideLines dbo.[WideLineList];
	DECLARE @ScriptLines dbo.LineList, @ScriptEntries dbo.EntryList;
	DECLARE @PreprocessedLines dbo.LineList, @PreprocessedEntries dbo.EntryList;
	Declare @PreScript NVARCHAR(MAX) =N'
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
	-- Fill entries from Document header, and Tabs headers
/*	-- To be discussed if it makes sense. If so, implement in C#.
	-- If Agent Id in Documents is not Null, then propagate it to the entries where the Agent Definition is compatible
	UPDATE PE
	SET AgentId = D.AgentId
	FROM @PreprocessedEntries PE
	JOIN @PreprocessedLines L ON PE.[LineIndex] = L.[Index] AND PE.[DocumentIndex] = L.[DocumentIndex]
	JOIN @Documents D ON L.DocumentIndex = D.[Index]
	JOIN dbo.LineDefinitionEntries LDE ON PE.EntryNumber = LDE.EntryNumber AND L.DefinitionId = LDE.LineDefinitionId
	JOIN dbo.Agents AG ON D.AgentId = AG.Id
	WHERE LDE.[AgentDefinitionId] LIKE N'%' + AG.DefinitionId +'%'
*/

	-- Get line definition which have script to run
	INSERT INTO @ScriptLineDefinitions
	SELECT DISTINCT DefinitionId FROM @Lines
	WHERE DefinitionId IN (
		SELECT [Id] FROM dbo.LineDefinitions
		WHERE [Script] IS NOT NULL
	);

	INSERT INTO @PreprocessedLines SELECT * FROM @Lines WHERE DefinitionId NOT IN (SELECT [Id] FROM @ScriptLineDefinitions)
	INSERT INTO @PreprocessedEntries
	SELECT E.* FROM @Entries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]

	IF EXISTS (SELECT * FROM @ScriptLineDefinitions)
	BEGIN
		INSERT INTO @ScriptLines SELECT * FROM @Lines WHERE DefinitionId IN (SELECT [Id] FROM @ScriptLineDefinitions)
		INSERT INTO @ScriptEntries
		SELECT E.* FROM @Entries E
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
			SELECT @Script = @PreScript + ISNULL([Script],N'') + @PostScript
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
	END -- IF EXISTS (SELECT * FROM @ScriptLineDefinitions)
-- PreprocessedLines and PreprocessedEntries now populated from script

-- Copy information from Line definitions to Entries
	UPDATE E
	SET
		E.[Direction] = COALESCE(E.[Direction], LDE.[Direction])
		-- TODO: fill with all the remaining defaults
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.LineDefinitionEntries LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[EntryNumber] = LDE.[EntryNumber]
	WHERE L.[DefinitionId] <> N'ManualLine';
	-- Copy information from Account to entries
	UPDATE E 
	SET
		E.[CurrencyId]				= COALESCE(A.[CurrencyId], E.[CurrencyId]),
		E.[AgentId]					= COALESCE(A.[AgentId], E.[AgentId]),
		E.[ResourceId]				= COALESCE(A.[ResourceId], E.[ResourceId]),
		E.[ResponsibilityCenterId]	= COALESCE(A.[ResponsibilityCenterId], E.[ResponsibilityCenterId]),
	--	E.[AccountIdentifier]		= COALESCE(A.[Identifier], E.[AccountIdentifier]),
		E.[EntryTypeId]				= COALESCE(A.[EntryTypeId], E.[EntryTypeId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Accounts A ON E.AccountId = A.Id
	WHERE L.DefinitionId = N'ManualLine';
	-- for all lines, Get currency and identifier from Resources if available.
	UPDATE E 
	SET
		E.[CurrencyId]		= COALESCE(R.[CurrencyId], E.[CurrencyId]),
		E.[MonetaryValue]	= COALESCE(R.[MonetaryValue], E.[MonetaryValue])
	--	E.[ResourceIdentifier]	=	COALESCE(R.[Identifier], E.[ResourceIdentifier]),
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Resources R ON E.ResourceId = R.Id;
	-- When the resource has exactly one non-null unit Id, set it as the Entry's UnitId
	WITH RU AS (
		SELECT [ResourceId], MIN(UnitId) AS UnitId
		FROM dbo.ResourceUnits
		GROUP BY [ResourceId]
		HAVING COUNT(*) = 1
	)
	UPDATE E
	SET E.[UnitId] = RU.UnitId
	FROM @PreprocessedEntries E
	JOIN RU ON E.ResourceId = RU.ResourceId;
	-- When currency is null, set it to functiona
	UPDATE @PreprocessedEntries
	SET CurrencyId = COALESCE(CurrencyId, @FunctionalCurrencyId);
	-- when there is only one responsibility center, use it everywhere
	IF (SELECT COUNT(*) FROM dbo.ResponsibilityCenters WHERE IsActive = 1) = 1
	UPDATE @PreprocessedEntries
	SET ResponsibilityCenterId = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE IsActive = 1);
	-- for financial amounts in functional currency, the value is known
	UPDATE E 
	SET
		[Value]			= [MonetaryValue]
	FROM @PreprocessedEntries E
	WHERE
		[CurrencyId] = @FunctionalCurrencyId
		AND (
			[Value] IS NULL OR 
			[Value] IS NOT NULL AND [Value] <> [MonetaryValue]
		);
	-- For financial amounts in foreign currency, the value is manually entered or read from a web service
	--UPDATE E 
	--SET E.[Value] = dbo.[fn_CurrencyExchange](D.[DocumentDate], A.[CurrencyId], @FunctionalCurrencyId, E.[MonetaryValue])
	--FROM @PreprocessedEntries E
	--JOIN dbo.Resources R ON E.ResourceId = R.Id
	--JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	--JOIN @Documents D ON L.DocumentIndex = D.[Index]
	--WHERE
	--	A.[CurrencyId] <> @FunctionalCurrencyId
	--	AND (E.[Value] <> dbo.[fn_CurrencyExchange](D.[DocumentDate], A.[CurrencyId], @FunctionalCurrencyId, E.[MonetaryValue]));

	-- TODO: Currently it sets the account to the first conformant
	-- We better do it so that, if the stored account is one of the conformants, it leaves it.
	-- else, it assigns the first acceptable one.
	WITH ConformantAccounts AS (
		SELECT MIN(A.[Id]) AS AccountId, E.[Index], E.[LineIndex], E.[DocumentIndex]
		FROM @PreprocessedEntries E
		JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
		JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
		JOIN dbo.Accounts A ON A.AccountTypeId IN (
			SELECT [Id] FROM AccountTypes 
			WHERE [Node].IsDescendantOf((
				SELECT [Node]
				FROM dbo.AccountTypes  
				WHERE [Code] = LDE.AccountTypeParentCode
			)) = 1
			)
			AND (A.[ResponsibilityCenterId] IS NULL OR A.[ResponsibilityCenterId] = E.[ResponsibilityCenterId])
			AND (A.[AgentId] IS NULL				OR A.[AgentId] = E.[AgentId])
			AND (A.[ResourceId] IS NULL				OR A.[ResourceId] = E.[ResourceId])
			AND (A.[CurrencyId] IS NULL				OR A.[CurrencyId] = E.[CurrencyId])
			AND (A.[EntryTypeId] IS NULL			OR A.[EntryTypeId] = E.[EntryTypeId])
			--AND (A.[Identifier] IS NULL				OR A.[Identifier] = E.[AccountIdentifier])
			--AND A.IsCurrent = LDE.IsCurrent
		WHERE L.DefinitionId <> N'ManualLine'
		AND A.IsDeprecated = 0
		GROUP BY  E.[Index], E.[LineIndex], E.[DocumentIndex]
	)
	UPDATE E
	SET E.AccountId = CA.AccountId
	FROM @PreprocessedEntries E
	JOIN ConformantAccounts CA ON E.[Index] = CA.[Index] AND E.[LineIndex] = CA.LineIndex AND E.[DocumentIndex] = CA.[DocumentIndex]
	-- Return the populated entries. (Later we may need to return the populated lines as well)
	SELECT @PreprocessedEntriesJson = 
	(
		SELECT *
		FROM @PreprocessedEntries
		FOR JSON PATH
	);
	--PRINT N'bll.Documents__Preprocess: PreprocessedEntriesJson = ' + ISNULL(@PreprocessedEntriesJson, N'');
	SELECT * FROM @PreprocessedEntries;
END