CREATE PROCEDURE [bll].[Documents__Preprocess]
	@DefinitionId NVARCHAR(50),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].EntryList READONLY,
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
	DECLARE @ScriptWideLines dbo.WideLineList, @ScriptLineDefinitions dbo.StringList, @LineDefinitionId NVARCHAR(50);
	DECLARE @WL dbo.[WideLineList], @PreprocessedWideLines dbo.[WideLineList];
	DECLARE @ScriptLines dbo.LineList, @ScriptEntries dbo.EntryList;
	DECLARE @PreprocessedLines dbo.LineList, @PreprocessedEntries dbo.EntryList;
	DECLARE @L [dbo].[LineList], @E [dbo].EntryList;
	DECLARE @Today DATE = CAST(GETDATE() AS DATE);
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
	INSERT INTO @L SELECT * FROM @Lines;
	INSERT INTO @E SELECT * FROM @Entries;
BEGIN --  Overwrite input with DB data that is read only
	UPDATE E
	SET E.CurrencyId = BE.CurrencyId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'CurrencyId';
	UPDATE E
	SET E.AgentId = BE.AgentId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'AgentId';
	UPDATE E
	SET E.ResourceId = BE.ResourceId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'ResourceId';
	UPDATE E
	SET E.AgentId = BE.AgentId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'AgentId';
	UPDATE E
	SET E.CenterId = BE.CenterId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'CenterId';
	UPDATE E
	SET E.AgentId = BE.AgentId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'AgentId';
	UPDATE E
	SET E.DueDate = BE.DueDate
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'DueDate';
	UPDATE E
	SET E.MonetaryValue = BE.MonetaryValue
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'MonetaryValue';
	UPDATE E
	SET E.Quantity = BE.Quantity
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'Quantity';
	UPDATE E
	SET E.UnitId = BE.UnitId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'UnitId';
	UPDATE E
	SET E.Time1 = BE.Time1
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'Time1';
	UPDATE E
	SET E.Time2 = BE.Time2
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'Time2';
	UPDATE E
	SET E.ExternalReference = BE.ExternalReference
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'ExternalReference';
	UPDATE E
	SET E.AdditionalReference = BE.AdditionalReference
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'AdditionalReference';
	UPDATE E
	SET E.NotedAgentId = BE.NotedAgentId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'NotedAgentId';
	UPDATE E
	SET E.NotedAgentName = BE.NotedAgentName
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'NotedAgentName';
	UPDATE E
	SET E.NotedAmount = BE.NotedAmount
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'NotedAmount';
	UPDATE E
	SET E.NotedDate = BE.NotedDate
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE LDC.ReadOnlyState <= BL.[State]
	AND LDC.ColumnName = N'NotedDate';
END
	-- Get line definition which have script to run
	INSERT INTO @ScriptLineDefinitions
	SELECT DISTINCT DefinitionId FROM @L
	WHERE DefinitionId IN (
		SELECT [Id] FROM dbo.LineDefinitions
		WHERE [Script] IS NOT NULL
	);
	-- Copy lines and entries with no script as they are
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
	END
	-- Copy information from Line definitions to Entries
	UPDATE E
	SET
	--	E.[Direction] = LDE.[Direction], -- Handled in C#
		E.[EntryTypeId] = COALESCE((SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = LDE.[EntryTypeCode]),
									E.[EntryTypeId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.LineDefinitionEntries LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[Index] = LDE.[Index]
	WHERE L.[DefinitionId] <> N'ManualLine';
	-- Copy information from Account to entries
	UPDATE E 
	SET
		E.[CurrencyId]				= COALESCE(A.[CurrencyId], E.[CurrencyId]),
		E.[AgentId]					= COALESCE(A.[AgentId], E.[AgentId]),
		E.[ResourceId]				= COALESCE(A.[ResourceId], E.[ResourceId]),
		E.[CenterId]				= COALESCE(A.[CenterId], E.[CenterId]),
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
	-- When currency is null, set it to functional currency
	--UPDATE @PreprocessedEntries
	--SET CurrencyId = COALESCE(CurrencyId, @FunctionalCurrencyId);

	DECLARE @BalanceSheetRoot HIERARCHYID = (
			SELECT [Node] FROM dbo.AccountTypes
			WHERE [Code] = N'StatementOfFinancialPositionAbstract'
	);

	-- When there is only one center, use it everywhere
	IF (SELECT COUNT(*) FROM dbo.[Centers] WHERE IsActive = 1 AND IsLeaf = 1) = 1
		UPDATE @PreprocessedEntries
		SET [CenterId] = (SELECT [Id] FROM dbo.[Centers] WHERE IsActive = 1 AND IsLeaf = 1);
	ELSE IF (SELECT COUNT(*) FROM dbo.[Centers] WHERE [CenterType] = N'Investment' AND IsActive = 1 AND IsLeaf = 1) = 1
		UPDATE PE 
		SET PE.CenterId = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] = N'Investment' AND IsActive = 1 AND IsLeaf = 1)
		FROM @PreprocessedEntries PE
		JOIN dbo.Accounts A ON PE.AccountId = A.[Id]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.AccountTypeId
		WHERE AC.[Node].IsDescendantOf(@BalanceSheetRoot) = 1
	-- For financial amounts in foreign currency, the rate is manually entered or read from a web service
	UPDATE E 
	SET E.[Value] = ER.[Rate] * E.[MonetaryValue]
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN @Documents D ON L.DocumentIndex = D.[Index]
	JOIN dbo.ExchangeRatesView ER ON E.CurrencyId = ER.CurrencyId
	WHERE
		ER.ValidAsOf <= ISNULL(D.[PostingDate], @Today)
	AND ER.ValidTill >	ISNULL(D.[PostingDate], @Today)
	AND L.[DefinitionId] <> N'ManualLine';

	-- TODO: Currently it sets the account to the first conformant
	-- We better do it so that, if the stored account is one of the conformants, it leaves it.
	-- else, it assigns the first acceptable one.
	WITH ConformantAccounts AS (
		SELECT MIN(A.[Id]) AS AccountId, E.[Index], E.[LineIndex], E.[DocumentIndex]
		FROM @PreprocessedEntries E
		JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
		JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.[Index] = LDE.[Index]
		JOIN dbo.Accounts A ON A.AccountTypeId IN (
			SELECT [Id] FROM AccountTypes 
			WHERE [Node].IsDescendantOf((
				SELECT [Node]
				FROM dbo.AccountTypes  
				WHERE [Code] = LDE.AccountTypeParentCode
			)) = 1
			)
			AND (A.[CenterId] IS NULL				OR A.[CenterId] = E.[CenterId])
			AND (A.[AgentId] IS NULL				OR A.[AgentId] = E.[AgentId])
			AND (A.[ResourceId] IS NULL				OR A.[ResourceId] = E.[ResourceId])
			AND (A.[CurrencyId] IS NULL				OR A.[CurrencyId] = E.[CurrencyId])
			AND (A.[EntryTypeId] IS NULL			OR A.[EntryTypeId] = E.[EntryTypeId])
			--AND (A.[Identifier] IS NULL			OR A.[Identifier] = E.[AccountIdentifier])
			AND (LDE.[IsCurrent] IS NULL			OR A.IsCurrent = LDE.IsCurrent)
		WHERE L.DefinitionId <> N'ManualLine'
		AND A.IsDeprecated = 0
		GROUP BY  E.[Index], E.[LineIndex], E.[DocumentIndex]
	)
	UPDATE E
	SET E.AccountId = CA.AccountId
	FROM @PreprocessedEntries E
	JOIN ConformantAccounts CA ON E.[Index] = CA.[Index] AND E.[LineIndex] = CA.LineIndex AND E.[DocumentIndex] = CA.[DocumentIndex]
	-- Return the populated entries.
	-- (Later we may need to return the populated lines and documents as well)
	SELECT @PreprocessedEntriesJson = 
	(
		SELECT *
		FROM @PreprocessedEntries
		FOR JSON PATH
	);	
	SELECT * FROM @PreprocessedEntries;
END

	--=-=-=-=-=-=- [C# Preprocessing after SQL]
	/* 
	
	 [✓] For Smart Lines: If CurrencyId == functional set Value = MonetaryValue
	 [✓] For Manual Lines: If CurrencyId == functional set MonetaryValue = Value

	*/