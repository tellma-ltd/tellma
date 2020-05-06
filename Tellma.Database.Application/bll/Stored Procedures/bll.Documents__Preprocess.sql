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
	DECLARE @ManualLineDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');

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
BEGIN --  Overwrite input with DB data that is read only
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
	WHERE L.[DefinitionId] <> @ManualLineDef;
	-- Copy information from Account to entries
	UPDATE E 
	SET
		E.[CurrencyId]		= COALESCE(A.[CurrencyId], E.[CurrencyId]),
		E.[ContractId]		= COALESCE(A.[ContractId], E.[ContractId]),
		E.[ResourceId]		= COALESCE(A.[ResourceId], E.[ResourceId]),
		E.[CenterId]		= COALESCE(A.[CenterId], E.[CenterId]),
		E.[EntryTypeId]		= COALESCE(A.[EntryTypeId], E.[EntryTypeId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN dbo.Accounts A ON E.AccountId = A.Id;
	-- for all lines, Get currency from Resources if available.
	UPDATE E 
	SET
		E.[CurrencyId]		= COALESCE(R.[CurrencyId], E.[CurrencyId]),
		E.[MonetaryValue]	= COALESCE(R.[MonetaryValue], E.[MonetaryValue])
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

	DECLARE @BalanceSheetRoot HIERARCHYID = (
			SELECT [Node] FROM dbo.AccountTypes
			WHERE [Code] = N'StatementOfFinancialPositionAbstract'
	);
	DECLARE @PropertyPlantAndEquipment HIERARCHYID = (
			SELECT [Node] FROM dbo.AccountTypes
			WHERE [Code] = N'PropertyPlantAndEquipment'
	);
	
	-- When there is only one center, use it everywhere
	IF (SELECT COUNT(*) FROM dbo.[Centers] WHERE [IsActive] = 1 AND [IsLeaf] = 1) = 1
	BEGIN
		UPDATE @PreprocessedDocuments
		SET [InvestmentCenterId] = (SELECT [Id] FROM dbo.[Centers] WHERE [IsActive] = 1 AND [IsLeaf] = 1);
		UPDATE @PreprocessedEntries
		SET [CenterId] = (SELECT [Id] FROM dbo.[Centers] WHERE [IsActive] = 1 AND [IsLeaf] = 1);
	END
	ELSE IF (SELECT COUNT(*) FROM dbo.[Centers] WHERE [CenterType] = N'Investment' AND [IsActive] = 1 AND [IsLeaf] = 1) = 1
	BEGIN
		DECLARE @InvestmentCenterId INT = (
			SELECT [Id]	FROM dbo.[Centers]
			WHERE [CenterType] = N'Investment'
			AND [IsActive] = 1 AND [IsLeaf] = 1
		);
		UPDATE @PreprocessedDocuments
		SET [InvestmentCenterId] = @InvestmentCenterId
		WHERE InvestmentCenterIsCommon = 1;
		-- Manual Lines
		UPDATE PE 
		SET PE.CenterId = @InvestmentCenterId
		FROM @PreprocessedEntries PE
		JOIN dbo.Accounts A ON PE.AccountId = A.[Id]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[IfrsTypeId]
		WHERE AC.[Node].IsDescendantOf(@BalanceSheetRoot) = 1
		AND AC.[Node].IsDescendantOf(@PropertyPlantAndEquipment) = 0;
		-- Smart Lines
		--UPDATE PE 
		--SET PE.CenterId = @InvestmentCenterId
		--FROM @PreprocessedEntries PE
		--JOIN @PreprocessedLines L ON PE.LineIndex = L.[Index] AND PE.[DocumentIndex] = L.[DocumentIndex]
		--JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND PE.[Index] = LDE.[Index]
		--JOIN dbo.AccountTypes AC ON AC.[Id] = LDE.AccountTypeParentId
		--WHERE AC.[Node].IsDescendantOf(@BalanceSheetRoot) = 1
		--AND AC.[Node].IsDescendantOf(@PropertyPlantAndEquipment) = 0
		--AND L.DefinitionId <> @ManualLineDef;
	END
	-- For financial amounts in foreign currency, the rate is manually entered or read from a web service
	UPDATE E 
	SET E.[Value] = ROUND(ER.[Rate] * E.[MonetaryValue], C.[E])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
--	JOIN @Documents D ON L.DocumentIndex = D.[Index]
	JOIN [map].[ExchangeRates]() ER ON E.CurrencyId = ER.CurrencyId
	JOIN dbo.Currencies C ON E.CurrencyId = C.[Id]
	WHERE
		ER.ValidAsOf <= ISNULL(L.[PostingDate], @Today)
	AND ER.ValidTill >	ISNULL(L.[PostingDate], @Today)
	AND L.[DefinitionId] <> @ManualLineDef;

	-- TODO: Currently it sets the account to the first conformant
	-- We better do it so that, if the stored account is one of the conformants, it leaves it.
	-- else, it assigns the first acceptable one.


	WITH ConformantAccounts AS (
		SELECT AM.AccountId, E.[Index], E.[LineIndex], E.[DocumentIndex]
		FROM @PreprocessedEntries E
		JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
		JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.[Index] = LDE.[Index]
		JOIN dbo.AccountDefinitions AD ON AD.Id = LDE.[AccountDefinitionId]
		JOIN dbo.AccountMappings AM ON
		-- 0: Direct Map
			(AD.[MapFunction] = 0 AND AD.[Id] = AM.AccountDefinitionId)
		-- 1: By Contract
		OR	(AD.[MapFunction] = 1 AND AD.[Id] = AM.AccountDefinitionId AND E.ContractId = AM.ContractId)
		-- 2: By Resource
		OR	(AD.[MapFunction] = 2 AND AD.[Id] = AM.AccountDefinitionId AND E.[ResourceId] = AM.[ResourceId])
		-- 3: By Center
		OR	(AD.[MapFunction] = 3 AND AD.[Id] = AM.AccountDefinitionId AND E.[CenterId] = AM.[CenterId])
		WHERE L.DefinitionId <> @ManualLineDef
		UNION
		SELECT AM.AccountId, E.[Index], E.[LineIndex], E.[DocumentIndex]
		FROM @PreprocessedEntries E
		JOIN @PreprocessedLines L ON E.LineIndex = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
		JOIN dbo.LineDefinitionEntries LDE ON L.DefinitionId = LDE.LineDefinitionId AND E.[Index] = LDE.[Index]
		JOIN dbo.AccountDefinitions AD ON AD.Id = LDE.[AccountDefinitionId]
		JOIN dbo.Resources R ON E.[ResourceId] = R.[Id]
		JOIN dbo.AccountMappings AM ON
		-- 21: By Resource Lookup1
			(AD.[MapFunction] = 21 AND AD.[Id] = AM.AccountDefinitionId AND R.Lookup1Id = AM.ResourceLookup1Id)
		-- 22: By Resource Lookup1 and Contract Id
		OR	(AD.[MapFunction] = 22 AND AD.[Id] = AM.AccountDefinitionId AND R.Lookup1Id = AM.ResourceLookup1Id AND AM.ContractId = E.ContractId)
		WHERE L.DefinitionId <> @ManualLineDef
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