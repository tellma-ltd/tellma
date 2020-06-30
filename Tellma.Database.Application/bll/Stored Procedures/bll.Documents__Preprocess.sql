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

	IF (SELECT COUNT(*) FROM dbo.Centers WHERE CenterType = N'Segment' AND IsActive = 1) = 1
	BEGIN
		DECLARE @SegmentId INT = (SELECT [Id] FROM dbo.Centers WHERE CenterType = N'Segment' AND IsActive = 1);
		UPDATE @D SET [SegmentId] = @SegmentId
	END
BEGIN
--	Overwrite input with data specified in the template (or clause)
	UPDATE E
	SET
		E.[Direction]		= COALESCE(ES.[Direction], E.[Direction]),
		E.[AccountId]		= COALESCE(ES.[AccountId], E.[AccountId]),
		E.[CurrencyId]		= COALESCE(ES.[CurrencyId], E.[CurrencyId]),
		E.[ContractId]		= COALESCE(ES.[ContractId], E.[ContractId]),
		E.[ResourceId]		= COALESCE(ES.[ResourceId], E.[ResourceId]),
		E.[CenterId]		= COALESCE(ES.[CenterId], E.[CenterId]),
		E.[EntryTypeId]		= COALESCE(ES.[EntryTypeId], E.[EntryTypeId]),
		E.[DueDate]			= COALESCE(ES.[DueDate], E.[DueDate]),
		E.[MonetaryValue]	= COALESCE(L.[Multiplier] * ES.[MonetaryValue], E.[MonetaryValue]),
		E.[Quantity]		= COALESCE(L.[Multiplier] * ES.[Quantity], E.[Quantity]),
		E.[UnitId]			= COALESCE(ES.[UnitId], E.[UnitId]),
--		E.[Value]			= COALESCE(L.[Multiplier] * ES.[Value], E.[Value]),
		E.[Time1]			= COALESCE(ES.[Time1], E.[Time1]),
		E.[Time2]			= COALESCE(ES.[Time2], E.[Time2]),
		E.[ExternalReference]= COALESCE(ES.[ExternalReference], E.[ExternalReference]),
		E.[AdditionalReference]= COALESCE(ES.[AdditionalReference], E.[AdditionalReference]),
		E.[NotedContractId]	= COALESCE(ES.[NotedContractId], E.[NotedContractId]),
		E.[NotedAgentName]	= COALESCE(ES.[NotedAgentName], E.[NotedAgentName]),
		E.[NotedAmount]		= COALESCE(ES.[NotedAmount], E.[NotedAmount]),
		E.[NotedDate]		= COALESCE(ES.[NotedDate], E.[NotedDate])
	FROM @E E
	JOIN @L L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Lines LS ON L.[TemplateLineId] = LS.[Id]
	JOIN dbo.Entries ES ON ES.[LineId] = LS.[Id]
	WHERE E.[Index] = ES.[Index]
  --  Overwrite input with DB data that is read only
	-- TODO : Overwrite readonly Memo
	UPDATE E
	SET E.CurrencyId = BE.CurrencyId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'CurrencyId';
	UPDATE E
	SET E.[ContractId] = BE.[ContractId]
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'ContractId';
	UPDATE E
	SET E.ResourceId = BE.ResourceId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'ResourceId';
	UPDATE E
	SET E.CenterId = BE.CenterId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'CenterId';
	UPDATE E
	SET E.EntryTypeId = BE.EntryTypeId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'EntryTypeId';
	UPDATE E
	SET E.DueDate = BE.DueDate
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'DueDate';
	UPDATE E
	SET E.MonetaryValue = BE.MonetaryValue
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'MonetaryValue';
	UPDATE E
	SET E.Quantity = BE.Quantity
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'Quantity';
	UPDATE E
	SET E.UnitId = BE.UnitId
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'UnitId';
	UPDATE E
	SET E.Time1 = BE.Time1
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'Time1';
	UPDATE E
	SET E.Time2 = BE.Time2
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'Time2';
	UPDATE E
	SET E.ExternalReference = BE.ExternalReference
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'ExternalReference';
	UPDATE E
	SET E.AdditionalReference = BE.AdditionalReference
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'AdditionalReference';
	UPDATE E
	SET E.[NotedContractId] = BE.[NotedContractId]
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'NotedContractId';
	UPDATE E
	SET E.NotedAgentName = BE.NotedAgentName
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'NotedAgentName';
	UPDATE E
	SET E.NotedAmount = BE.NotedAmount
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'NotedAmount';
	UPDATE E
	SET E.NotedDate = BE.NotedDate
	FROM @E E
	JOIN dbo.Entries BE ON E.Id = BE.Id
	JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
	JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
	WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	AND LDC.ColumnName = N'NotedDate';
END
	-- for all lines, Get currency and center from Resources if available.
	UPDATE E 
	SET
		E.[CenterId]		= COALESCE(R.[CenterId], E.[CenterId]),
		E.[CurrencyId]		= COALESCE(R.[CurrencyId], E.[CurrencyId]),
		E.[MonetaryValue]	= COALESCE(R.[MonetaryValue], E.[MonetaryValue])
	FROM @E E
	JOIN @L L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Resources R ON E.ResourceId = R.Id;
	-- for all lines, Get currency and center from Contracts if available.
	UPDATE E 
	SET
		E.[CenterId]		= COALESCE(C.[CenterId], E.[CenterId]),
		E.[CurrencyId]		= COALESCE(C.[CurrencyId], E.[CurrencyId])
	FROM @E E
	JOIN @L L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Contracts C ON E.ContractId = C.Id;
	-- When the resource has exactly one non-null unit Id, set it as the Entry's UnitId
	WITH RU AS (
		SELECT [ResourceId], MIN(UnitId) AS UnitId
		FROM dbo.ResourceUnits
		GROUP BY [ResourceId]
		HAVING COUNT(*) = 1
	)
	UPDATE E
	SET E.[UnitId] = RU.UnitId
	FROM @E E
	JOIN RU ON E.ResourceId = RU.ResourceId;
	-- Copy information from Account to entries
	UPDATE E 
	SET
		E.[CurrencyId]		= COALESCE(A.[CurrencyId], E.[CurrencyId]),
		E.[ContractId]		= COALESCE(A.[ContractId], E.[ContractId]),
		E.[ResourceId]		= COALESCE(A.[ResourceId], E.[ResourceId]),
		E.[CenterId]		= COALESCE(A.[CenterId], E.[CenterId]),
		E.[EntryTypeId]		= COALESCE(A.[EntryTypeId], E.[EntryTypeId])
	FROM @E E
	JOIN @L L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Accounts A ON E.AccountId = A.Id;

	-- Get line definition which have script to run
	INSERT INTO @ScriptLineDefinitions
	SELECT DISTINCT DefinitionId FROM @L
	WHERE DefinitionId IN (
		SELECT [Id] FROM dbo.LineDefinitions
		WHERE [Script] IS NOT NULL
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
		E.[EntryTypeId] = COALESCE(LDE.[EntryTypeId], E.[EntryTypeId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.LineDefinitionEntries LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[Index] = LDE.[Index]
	WHERE L.[DefinitionId] <> @ManualLineLD;

	-- for each account type, if there is only one compatible center, use it
	WITH SingleCenterEntries AS (
		SELECT E.[Index], E.[LineIndex], E.[DocumentIndex], C.[CenterType]
		FROM @PreprocessedEntries E
		JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
		JOIN dbo.[LineDefinitionEntries] LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[Index] = LDE.[Index]
		JOIN dbo.[AccountTypeCenterTypes] ATCT ON LDE.AccountTypeId = ATCT.AccountTypeId
		JOIN dbo.Centers C ON ATCT.CenterType = C.CenterType
		WHERE C.[IsActive] = 1
		GROUP BY E.[Index], E.[LineIndex], E.[DocumentIndex], C.[CenterType]
		HAVING COUNT(*) = 1
	)
	UPDATE E
	SET E.[CenterId] = C.[Id]
	FROM @PreprocessedEntries E
	JOIN SingleCenterEntries SCE ON E.[Index] = SCE.[Index] AND E.[LineIndex] = SCE.[LineIndex] AND E.[DocumentIndex] = SCE.[DocumentIndex]
	JOIN dbo.Centers C ON SCE.[CenterType] = C.[CenterType]
	WHERE C.[IsActive] = 1
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
	AND L.[DefinitionId] <> @ManualLineLD;

	-- Set the Account based on provided info so far
	With LineEntries AS (
		SELECT E.[Index], E.[LineIndex], E.[DocumentIndex], ATC.[Id] AS [AccountTypeId], R.[DefinitionId] AS ResourceDefinitionId, E.[ResourceId],
				C.[DefinitionId] AS ContractDefinitionId, E.[ContractId], E.[CenterId], E.[CurrencyId]
		FROM @PreprocessedEntries E
		JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
		JOIN dbo.[LineDefinitionEntries] LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[Index] = LDE.[Index]
		JOIN dbo.AccountTypes ATP ON LDE.[AccountTypeId] = ATP.[Id]
		JOIN dbo.AccountTypes ATC ON ATC.[Node].IsDescendantOf(ATP.[Node]) = 1
		LEFT JOIN dbo.Resources R ON E.[ResourceId] = R.[Id]
		LEFT JOIN dbo.Contracts C ON E.[ContractId] = C.[Id]
		--WHERE (R.[DefinitionId] IS NULL OR R.[DefinitionId] IN (
		--	SELECT [ResourceDefinitionId] FROM [LineDefinitionEntryResourceDefinitions]
		--	WHERE [LineDefinitionEntryId] = LDE.[Id]
		--))
		--AND (C.[DefinitionId] IS NULL OR C.[DefinitionId] IN (
		--	SELECT [ContractDefinitionId] FROM [LineDefinitionEntryContractDefinitions]
		--	WHERE [LineDefinitionEntryId] = LDE.[Id]		
		--))
	),
	ConformantAccounts AS (
		SELECT LE.[Index], LE.[LineIndex], LE.[DocumentIndex], MIN(A.Id) AS MINAccountId, MAX(A.[Id]) AS MAXAccountId
		FROM dbo.Accounts A
		JOIN LineEntries LE ON LE.[AccountTypeId] = A.[AccountTypeId]
		WHERE
			(A.[CenterId] IS NULL OR A.[CenterId] = LE.[CenterId])
		AND (A.[CurrencyId] IS NULL OR A.[CurrencyId] = LE.[CurrencyId])
		AND (A.[ResourceDefinitionId] IS NULL AND LE.[ResourceDefinitionId] IS NULL OR A.[ResourceDefinitionId] = LE.[ResourceDefinitionId])
		AND (A.[ResourceId] IS NULL OR A.[ResourceId] = LE.[ResourceId])
		AND (A.[ContractDefinitionId] IS NULL AND LE.[ContractDefinitionId] IS NULL OR A.[ContractDefinitionId] = LE.[ContractDefinitionId])
		AND (A.[ContractId] IS NULL OR A.[ContractId] = LE.[ContractId])
		GROUP BY LE.[Index], LE.[LineIndex], LE.[DocumentIndex]
	)
	UPDATE E -- Override the Account when there is exactly one solution. Otherwise, leave it.
	SET E.AccountId = CA.MINAccountId
	FROM @PreprocessedEntries E
	JOIN ConformantAccounts CA ON E.[Index] = CA.[Index] AND E.[LineIndex] = CA.[LineIndex] AND E.[DocumentIndex] = CA.[DocumentIndex]
	WHERE CA.MINAccountId = CA.MAXAccountId;

	With LineEntries2 AS (
		SELECT E.[Index], E.[LineIndex], E.[DocumentIndex], ATC.[Id] AS [AccountTypeId], R.[DefinitionId] AS ResourceDefinitionId, E.[ResourceId],
				C.[DefinitionId] AS ContractDefinitionId, E.[ContractId], E.[CenterId], E.[CurrencyId]
		FROM @PreprocessedEntries E
		JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
		JOIN dbo.[LineDefinitionEntries] LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[Index] = LDE.[Index]
		JOIN dbo.AccountTypes ATP ON LDE.[AccountTypeId] = ATP.[Id]
		JOIN dbo.AccountTypes ATC ON ATC.[Node].IsDescendantOf(ATP.[Node]) = 1
		LEFT JOIN dbo.Resources R ON E.[ResourceId] = R.[Id]
		LEFT JOIN dbo.Contracts C ON E.[ContractId] = C.[Id]
	),
	ConformantAccounts2 AS (
		SELECT LE.[Index], LE.[LineIndex], LE.[DocumentIndex], A.[Id] AS AccountId
		FROM dbo.Accounts A
		JOIN LineEntries2 LE ON LE.[AccountTypeId] = A.[AccountTypeId]
		WHERE
			(A.[CenterId] IS NULL OR A.[CenterId] = LE.[CenterId])
		AND (A.[CurrencyId] IS NULL OR A.[CurrencyId] = LE.[CurrencyId])
		AND (A.[ResourceDefinitionId] IS NULL AND LE.[ResourceDefinitionId] IS NULL OR A.[ResourceDefinitionId] = LE.[ResourceDefinitionId])
		AND (A.[ResourceId] IS NULL OR A.[ResourceId] = LE.[ResourceId])
		AND (A.[ContractDefinitionId] IS NULL AND LE.[ContractDefinitionId] IS NULL OR A.[ContractDefinitionId] = LE.[ContractDefinitionId])
		AND (A.[ContractId] IS NULL OR A.[ContractId] = LE.[ContractId])
	)
	UPDATE E -- Set account to null, if non conformant
	SET E.AccountId = NULL
	FROM @PreprocessedEntries E
	LEFT JOIN ConformantAccounts2 CA
	ON E.[Index] = CA.[Index] AND E.[LineIndex] = CA.[LineIndex] AND E.[DocumentIndex] = CA.[DocumentIndex] AND E.AccountId = CA.AccountId
	WHERE E.AccountId IS NOT NULL AND  CA.AccountId IS NULL;

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