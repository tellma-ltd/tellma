CREATE PROCEDURE [bll].[Documents__Preprocess]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].[EntryList] READONLY,
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;

	--=-=-=-=-=-=- [C# Preprocessing before SQL]
	/* 
	 -- TODO: Update
	
	 [✓] If Clearance is NULL, set it to 0
	 [✓] If a line has the wrong number of entries, fix it
	 [✓] Set all Entries' Directions according to definition (except for manual lines)
	 [✓] Copy all IsCommon values from the documents to the lines and entries

	*/

	DECLARE @FunctionalCurrencyId NCHAR(3) = [dal].[fn_FunctionalCurrencyId]();
	DECLARE @ScriptWideLines [dbo].[WideLineList], @ScriptLineDefinitions [dbo].[StringList], @LineDefinitionId INT;
	DECLARE @WL [dbo].[WideLineList], @PreprocessedWideLines [dbo].[WideLineList];
	DECLARE @ScriptLines [dbo].[LineList], @ScriptEntries [dbo].[EntryList];
	DECLARE @PreprocessedDocuments [dbo].[DocumentList],@PreprocessedDocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList], 
			@PreprocessedLines [dbo].[LineList], @PreprocessedEntries [dbo].[EntryList];
	DECLARE @D [dbo].[DocumentList], @DLDE [dbo].[DocumentLineDefinitionEntryList],
			@L [dbo].[LineList], @E [dbo].[EntryList];
	DECLARE @Today DATE = CAST(GETDATE() AS DATE);
	DECLARE @ManualLineLD INT = ISNULL((SELECT [Id] FROM [dbo].[LineDefinitions] WHERE [Code] = N'ManualLine'),0);
	DECLARE @ExchangeVarianceLineLD INT = (SELECT [Id] FROM [dbo].[LineDefinitions] WHERE [Code] = N'ExchangeVariance');
	DECLARE @CostReallocationToInvestmentPropertyUnderConstructionOrDevelopmentLD INT = 
		(SELECT [Id] FROM [dbo].[LineDefinitions] WHERE [Code] = N'CostReallocationToInvestmentPropertyUnderConstructionOrDevelopment');

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
	INSERT INTO @DLDE SELECT * FROM @DocumentLineDefinitionEntries;
	INSERT INTO @L SELECT * FROM @Lines;
	INSERT INTO @E SELECT * FROM @Entries;

	IF (SELECT COUNT(*) FROM [dbo].[Centers] WHERE [CenterType] = N'BusinessUnit' AND [IsActive] = 1) = 1
	BEGIN
		DECLARE @BusinessUnitId INT = (SELECT [Id] FROM [dbo].[Centers] WHERE [CenterType] = N'BusinessUnit' AND [IsActive] = 1);
		UPDATE @D SET [CenterId] = @BusinessUnitId
	END
--	Remove Residuals
	UPDATE E
	SET E.[AgentId] = NULL
	FROM @E E
	JOIN @L L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[Accounts] A ON E.[AccountId] = A.Id
	WHERE A.[AgentDefinitionId] IS NULL
	AND L.[DefinitionId] = @ManualLineLD; -- I added this condition, because changing smart line definition for cash control was causing problems

	UPDATE E
	SET E.[ResourceId] = NULL--, E.Quantity = NULL, E.UnitId = NULL
	FROM @E E
	JOIN @L L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[Accounts] A ON E.[AccountId] = A.Id
	WHERE  A.ResourceDefinitionId IS NULL;

	UPDATE E
	SET E.[NotedAgentId] = NULL
	FROM @E E
	JOIN @L L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[Accounts] A ON E.[AccountId] = A.Id
	WHERE A.[NotedAgentDefinitionId] IS NULL
	AND L.[DefinitionId] = @ManualLineLD;

	UPDATE E
	SET E.[NotedResourceId] = NULL
	FROM @E E
	JOIN @L L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[Accounts] A ON E.[AccountId] = A.Id
	WHERE A.[NotedResourceDefinitionId] IS NULL
	AND L.[DefinitionId] = @ManualLineLD; 
	
	UPDATE E
	SET E.[EntryTypeId] = NULL
	FROM @E E
	JOIN @L L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[Accounts] A ON E.[AccountId] = A.Id
	JOIN [dbo].[AccountTypes] AC ON A.AccountTypeId = AC.Id
	WHERE AC.EntryTypeParentId IS NULL;

	-- TODO:  Remove labels, etc.

	-- Overwrite input with DB data that is read only
	-- TODO : Overwrite readonly Memo
	WITH CTE AS (
		SELECT
			E.[Index], E.[LineIndex], E.[DocumentIndex], E.[CurrencyId], E.[CenterId], E.[AgentId],
			E.[NotedAgentId], E.[ResourceId], E.[NotedResourceId], E.[Quantity], E.[UnitId],
			E.[MonetaryValue], E.[Time1], E.[Duration], E.[DurationUnitId] , E.[Time2],
			E.[ExternalReference], E.[ReferenceSourceId], E.[InternalReference], E.[NotedAgentName],  E.[NotedAmount],  E.[NotedDate], 
			E.[EntryTypeId], LDC.[ColumnName]
		FROM @E E
		JOIN [dbo].[Entries] BE ON E.Id = BE.Id
		JOIN [dbo].[Lines] BL ON BE.[LineId] = BL.[Id]
		JOIN [dbo].[LineDefinitionColumns] LDC ON BL.[DefinitionId] = LDC.[LineDefinitionId] AND LDC.[EntryIndex] = BE.[Index]
		WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
	)
	UPDATE E
	SET
		E.[CurrencyId]			= IIF(CTE.[ColumnName] = N'CurrencyId', CTE.[CurrencyId], E.[CurrencyId]),
		E.[CenterId]			= IIF(CTE.[ColumnName] = N'CenterId', CTE.[CenterId], E.[CenterId]),
		E.[AgentId]				= IIF(CTE.[ColumnName] = N'AgentId', CTE.[AgentId], E.[AgentId]),
		E.[NotedAgentId]		= IIF(CTE.[ColumnName] = N'NotedAgentId', CTE.[NotedAgentId], E.[NotedAgentId]),
		E.[ResourceId]			= IIF(CTE.[ColumnName] = N'ResourceId', CTE.[ResourceId], E.[ResourceId]),
		E.[NotedResourceId]		= IIF(CTE.[ColumnName] = N'NotedResourceId', CTE.[NotedResourceId], E.[NotedResourceId]),
		E.[Quantity]			= IIF(CTE.[ColumnName] = N'Quantity', CTE.[Quantity], E.[Quantity]),
		E.[UnitId]				= IIF(CTE.[ColumnName] = N'UnitId', CTE.[UnitId], E.[UnitId]),
		E.[MonetaryValue]		= IIF(CTE.[ColumnName] = N'MonetaryValue', CTE.[MonetaryValue], E.[MonetaryValue]),
		E.[Time1]				= IIF(CTE.[ColumnName] = N'Time1', CTE.[Time1], E.[Time1]),
		E.[Duration]			= IIF(CTE.[ColumnName] = N'Duration', CTE.[Duration], E.[Duration]),
		E.[DurationUnitId]		= IIF(CTE.[ColumnName] = N'DurationUnitId', CTE.[DurationUnitId], E.[DurationUnitId]),
		E.[Time2]				= IIF(CTE.[ColumnName] = N'Time2', CTE.[Time2], E.[Time2]),
		E.[ExternalReference]	= IIF(CTE.[ColumnName] = N'ExternalReference', CTE.[ExternalReference], E.[ExternalReference]),
		E.[ReferenceSourceId]	= IIF(CTE.[ColumnName] = N'ReferenceSourceId', CTE.[ReferenceSourceId], E.[ReferenceSourceId]),
		E.[InternalReference]	= IIF(CTE.[ColumnName] = N'InternalReference', CTE.[InternalReference], E.[InternalReference]),
		E.[NotedAgentName]		= IIF(CTE.[ColumnName] = N'NotedAgentName', CTE.[NotedAgentName], E.[NotedAgentName]),
		E.[NotedAmount]			= IIF(CTE.[ColumnName] = N'NotedAmount', CTE.[NotedAmount], E.[NotedAmount]),
		E.[NotedDate]			= IIF(CTE.[ColumnName] = N'NotedDate', CTE.[NotedDate], E.[NotedDate]),
		E.[EntryTypeId]			= IIF(CTE.[ColumnName] = N'EntryTypeId', CTE.[EntryTypeId], E.[EntryTypeId])
	FROM @E E
	JOIN CTE ON  E.[Index] = CTE.[Index] AND E.[LineIndex] = CTE.[LineIndex] AND E.[DocumentIndex] = CTE.[DocumentIndex];

	-- Get line definitions which have preprocess script to run
	INSERT INTO @ScriptLineDefinitions
	SELECT DISTINCT DefinitionId FROM @L
	WHERE DefinitionId IN (
		SELECT [Id] FROM [dbo].[LineDefinitions]
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
			FROM [dbo].[LineDefinitions] WHERE [Id] = @LineDefinitionId;

			DELETE FROM @WL;
			INSERT INTO @WL SELECT * FROM @ScriptWideLines WHERE [DefinitionId] = @LineDefinitionId;

			INSERT INTO @PreprocessedWideLines--** causes nested INSERT EXEC
			EXECUTE	sp_executesql @Script, N'@WideLines WideLineList READONLY', @WideLines = @WL;
			
			FETCH NEXT FROM LineDefinition_Cursor INTO @LineDefinitionId;
		END
		INSERT INTO @PreprocessedLines(
			[Index],[DocumentIndex],[Id], [DefinitionId], [PostingDate], [Memo], [Boolean1],[Decimal1],[Text1]
		)
		SELECT
			[Index],[DocumentIndex],[Id], [DefinitionId], [PostingDate], [Memo], [Boolean1],[Decimal1],[Text1]
		FROM @PreprocessedWideLines;
		INSERT INTO @PreprocessedEntries	
		EXEC bll.WideLines__Unpivot @PreprocessedWideLines
	END
	-- for all lines, Get currency and center from Resources
	DECLARE @BalanceSheetNode HIERARCHYID = (SELECT [Node] FROM [dbo].[AccountTypes] WHERE [Concept] = N'StatementOfFinancialPositionAbstract');
	DECLARE @ExpenseByNatureNode HIERARCHYID = (SELECT [Node] FROM [dbo].[AccountTypes] WHERE [Concept] = N'ExpenseByNature');

	--	Get center from resource, if any. This works for JV only or for smart screens specifying the account
	-- TODO make it depending on Account Type
	UPDATE E 
	SET
		E.[CenterId] = R.[CenterId]
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[Resources] R ON E.[ResourceId] = R.Id
	JOIN [map].[Accounts]() A ON E.[AccountId] = A.[Id] -- E.[AccountId] is NULL for most smart screens
	AND R.[CenterId] IS NOT NULL
	AND L.DefinitionId = @ManualLineLd

	-- A resource can have a business unit only (e.g., a customer check)
	-- or a POS center (e.g., a given product for sale)
	-- or an admin center (when the same product is used for internal consumption)
	-- So it is not always clear. However, we can copy the center's business unit in case the account type
	-- was a balance sheet account
	UPDATE E
	SET
		E.[CenterId] = dal.fn_Center__BusinessUnit(R.[CenterId]) -- if Not BU, we get the BU
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[LineDefinitionEntries] LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND LDE.[Index] = E.[Index]
	JOIN [dbo].[AccountTypes] AC ON LDE.[ParentAccountTypeId] = AC.[Id]
	JOIN [dbo].[Resources] R ON E.[ResourceId] = R.[Id]
	WHERE AC.[Node].IsDescendantOf(@BalanceSheetNode) = 1
	AND R.[CenterId] IS NOT NULL
	-- for all lines, get currency from resource (which is required), and monetary value, if any
	UPDATE E 
	SET
		E.[CurrencyId]		= R.[CurrencyId],
		E.[MonetaryValue]	= COALESCE(R.[MonetaryValue], E.[MonetaryValue]),
		E.[NotedAgentId]	= COALESCE(R.[ParticipantId], E.[NotedAgentId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[Resources] R ON E.[ResourceId] = R.[Id];

	-- for smart lines, Get center from Agents if available.
	UPDATE E 
	SET
		E.[CenterId]		= RL.[CenterId]
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[LineDefinitionEntries] LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND LDE.[Index] = E.[Index]
	JOIN [dbo].[AccountTypes] AC ON LDE.[ParentAccountTypeId] = AC.[Id]
	JOIN [dbo].[Agents] RL ON E.[AgentId] = RL.Id
	WHERE AC.[Node].IsDescendantOf(@BalanceSheetNode) = 1
	AND RL.[CenterId] IS NOT NULL

	-- for JV, Get Center from Agents if available
	UPDATE E 
	SET
		E.[CenterId]		= COALESCE(RL.[CenterId], E.[CenterId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[Agents] RL ON E.[AgentId] = RL.Id
	JOIN [dbo].[Accounts] A ON E.[AccountId] = A.[Id]
	JOIN [dbo].[AccountTypes] AC ON A.[AccountTypeId] = AC.[Id]
	WHERE AC.[Node].IsDescendantOf(@BalanceSheetNode) = 1

	-- for all lines, Get currency from Agents if available.
	UPDATE E 
	SET
		E.[CurrencyId]		= COALESCE(RL.[CurrencyId], E.[CurrencyId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[Agents] RL ON E.[AgentId] = RL.[Id]

	-- When the resource has exactly one non-null unit Id, and the account does not allow PureUnit set it as the Entry's UnitId
	UPDATE E
	SET E.[UnitId] = COALESCE(R.[UnitId], E.[UnitId])
	FROM @PreprocessedEntries E
	JOIN [dbo].[Resources] R ON E.[ResourceId] = R.[Id]
	JOIN [dbo].[ResourceDefinitions] RD ON R.[DefinitionId] = RD.[Id]
	--JOIN [dbo].[Accounts] A ON E.[AccountId] = A.[Id]
	--JOIN [dbo].[AccountTypes] AC ON A.[AccountTypeId] = AC.[Id]
	WHERE
		RD.[UnitCardinality] IN (N'Single', N'None')
	AND NOT (RD.ResourceDefinitionType IN (N'PropertyPlantAndEquipment', N'InvestmentProperty', N'IntangibleAssetsOtherThanGoodwill'));

	UPDATE E
	SET E.[Quantity] = 1
	FROM @PreprocessedEntries E
	JOIN [dbo].[Units] U ON E.[UnitId] = U.[Id]
	WHERE U.[UnitType] = N'Pure';
--	AND E.[Quantity] <>0;

	-- Copy information from Account to entries in Manual JV
	-- In Smart screens, we do not, otherwise changing the resource to one
	-- incompatible with the account will cause unit clashes
	UPDATE E 
	SET
		E.[CurrencyId]		= COALESCE(A.[CurrencyId], E.[CurrencyId]),
		E.[AgentId]			= COALESCE(A.[AgentId], E.[AgentId]),
		E.[NotedAgentId]	= COALESCE(A.[NotedAgentId], E.[NotedAgentId]),
		E.[ResourceId]		= COALESCE(A.[ResourceId], E.[ResourceId]),
		E.[NotedResourceId]	= COALESCE(A.[NotedResourceId], E.[NotedResourceId]),
		E.[CenterId]		= COALESCE(A.[CenterId], E.[CenterId]),
		E.[EntryTypeId]		= COALESCE(A.[EntryTypeId], E.[EntryTypeId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[Accounts] A ON E.[AccountId] = A.Id
	WHERE L.[DefinitionId] = @ManualLineLD; 

	-- Copy information from Line definitions to Entries
	UPDATE E
	SET
	--	E.[Direction] = LDE.[Direction], -- Handled in C#
		E.[EntryTypeId] = COALESCE(LDE.[EntryTypeId], E.[EntryTypeId])
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[LineDefinitionEntries] LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[Index] = LDE.[Index]
	WHERE L.[DefinitionId] <> @ManualLineLD;

	-- Compute Time2 based on Time1 and Duration
	--UPDATE E
	--SET Time2 = 
	--	CASE
	--	WHEN U.[Code] = N'yr' THEN DATEADD(DAY, -1, DATEADD(YEAR, E.[Duration], Time1))
	--	WHEN U.[Code] = N'mo' THEN DATEADD(DAY, -1, DATEADD(MONTH,  E.[Duration], Time1))
	--	ELSE Time1
	--	END
	--FROM @PreprocessedEntries E
	--JOIN dbo.Units U ON E.[DurationUnitId] = U.[Id]
	
	-- For financial amounts in foreign currency, the rate is manually set or read from a web service
	UPDATE E
	SET [MonetaryValue] = ROUND([MonetaryValue], C.E)
	FROM @PreprocessedEntries E
	JOIN [dbo].[Currencies] C ON E.[CurrencyId] = C.[Id]

	UPDATE E
	SET E.[Value] = [bll].[fn_ConvertCurrencies](
						L.[PostingDate], E.[CurrencyId], @FunctionalCurrencyId, E.[MonetaryValue]
					)
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	WHERE L.[DefinitionId] <> @ManualLineLD
	AND L.[DefinitionId] IN (SELECT [Id] FROM [dbo].[LineDefinitions] WHERE [GenerateScript] IS NULL);
	
	DECLARE @LineEntries TABLE (
				[Index] INT, 
				[LineIndex] INT, 
				[DocumentIndex] INT,  
				[AccountTypeId] INT, PRIMARY KEY ([Index], [LineIndex], [DocumentIndex], [AccountTypeId]),
				[AgentDefinitionId] INT,
				[AgentId] INT,
				[NotedAgentDefinitionId] INT,
				[NotedAgentId] INT,
				[ResourceDefinitionId] INT,
				[ResourceId] INT,
				[NotedResourceDefinitionId] INT,
				[NotedResourceId] INT,
				[CenterId] INT,
				[CurrencyId] NCHAR (3)
			)
	INSERT INTO @LineEntries([Index], [LineIndex], [DocumentIndex], [AccountTypeId],
					[AgentDefinitionId], [AgentId],
					[NotedAgentDefinitionId], [NotedAgentId],
					[ResourceDefinitionId], [ResourceId], 
					[NotedResourceDefinitionId], [NotedResourceId],
					[CenterId], [CurrencyId])
	SELECT E.[Index], E.[LineIndex], E.[DocumentIndex], ATC.[Id] AS [AccountTypeId],
			RL.[DefinitionId], E.[AgentId],
			NRL.[DefinitionId], E.[NotedAgentId],
			R.[DefinitionId] AS ResourceDefinitionId, E.[ResourceId],
			NR.[DefinitionId] AS NotedResourceDefinitionId, E.[NotedResourceId],
			E.[CenterId], E.[CurrencyId]
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN [dbo].[LineDefinitionEntries] LDE ON L.[DefinitionId] = LDE.[LineDefinitionId] AND E.[Index] = LDE.[Index]
	JOIN [dbo].[AccountTypes] ATP ON LDE.[ParentAccountTypeId] = ATP.[Id]
	JOIN [dbo].[AccountTypes] ATC ON ATC.[Node].IsDescendantOf(ATP.[Node]) = 1
	LEFT JOIN [dbo].[Resources] R ON R.[Id] = E.[ResourceId]
	LEFT JOIN [dbo].[Resources] NR ON  NR.[Id] = E.[NotedResourceId]
	LEFT JOIN [dbo].[Agents] RL ON RL.[Id] = E.[AgentId]
	LEFT JOIN [dbo].[Agents] NRL ON NRL.[Id] = E.[NotedAgentId]
	WHERE L.[DefinitionId] <> @ManualLineLD
	--TODO: By using Null Resource and Null Agent, we can speed up the following code by 3x, as we can then use INNER JOIN
--	AND (E.[AgentId] IS NOT NULL OR ATC.[AgentDefinitionId] IS NULL AND RL.[DefinitionId] IS NULL OR ATC.[AgentDefinitionId] = RL.[DefinitionId])
--	AND (E.[NotedAgentId] IS NOT NULL OR ATC.[NotedAgentDefinitionId] IS NULL AND NR.[DefinitionId] IS NULL OR ATC.[NotedAgentDefinitionId] = NR.[DefinitionId])

	AND ATC.[IsActive] = 1 AND ATC.[IsAssignable] = 1;

	-- Set the Account based on provided info so far
	DECLARE @ConformantAccounts TABLE(
		[Index]			INT,
		[LineIndex]		INT, 
		[DocumentIndex] INT, 
		[AccountId]		INT, PRIMARY KEY ([Index], [LineIndex], [DocumentIndex], [AccountId])
	);
	INSERT INTO @ConformantAccounts([Index], [LineIndex], [DocumentIndex], [AccountId])
	SELECT LE.[Index], LE.[LineIndex], LE.[DocumentIndex], A.[Id] AS AccountId
	FROM [dbo].[Accounts] A
	JOIN @LineEntries LE ON LE.[AccountTypeId] = A.[AccountTypeId]
	WHERE
		(A.[IsActive] = 1)
	AND	(A.[CenterId] IS NULL OR A.[CenterId] = LE.[CenterId])
	AND (A.[CurrencyId] IS NULL OR A.[CurrencyId] = LE.[CurrencyId])
	AND (A.[AgentDefinitionId] IS NULL AND LE.[AgentDefinitionId] IS NULL OR A.[AgentDefinitionId] = LE.[AgentDefinitionId])
	AND (A.[AgentId] IS NULL OR A.[AgentId] = LE.[AgentId])
	AND (A.[NotedAgentDefinitionId] IS NULL AND LE.[NotedAgentDefinitionId] IS NULL OR A.[NotedAgentDefinitionId] = LE.[NotedAgentDefinitionId])
	AND (A.[NotedAgentId] IS NULL OR A.[NotedAgentId] = LE.[NotedAgentId])
	AND (A.[ResourceDefinitionId] IS NULL AND LE.[ResourceDefinitionId] IS NULL OR A.[ResourceDefinitionId] = LE.[ResourceDefinitionId])
	AND (A.[ResourceId] IS NULL OR A.[ResourceId] = LE.[ResourceId])
	AND (A.[NotedResourceDefinitionId] IS NULL AND LE.[NotedResourceDefinitionId] IS NULL OR A.[NotedResourceDefinitionId] = LE.[NotedResourceDefinitionId])
	AND (A.[NotedResourceId] IS NULL OR A.[NotedResourceId] = LE.[NotedResourceId])
	
	DECLARE @ConformantAccountsSummary TABLE(
		[Index] INT, 
		[LineIndex] INT, 
		[DocumentIndex] INT, PRIMARY KEY ([Index], [LineIndex], [DocumentIndex]),
		[AccountId] INT INDEX [IX_ConformantAccounts_AccountId] NONCLUSTERED,
		[AccountCount] INT
	);
	INSERT INTO @ConformantAccountsSummary([Index], [LineIndex], [DocumentIndex], [AccountId], [AccountCount])
	SELECT [Index], [LineIndex], [DocumentIndex], MIN([AccountId]) AS AccountId, Count(*) AS AccountCount
	FROM @ConformantAccounts
	GROUP BY [Index], [LineIndex], [DocumentIndex]

	UPDATE E -- Override the Account when there is exactly one solution. Otherwise, leave it.
	SET E.[AccountId] = CAS.[AccountId]
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	JOIN @ConformantAccountsSummary CAS
	ON E.[Index] = CAS.[Index] AND E.[LineIndex] = CAS.[LineIndex] AND E.[DocumentIndex] = CAS.[DocumentIndex]
	WHERE L.[DefinitionId] <> @ManualLineLD
	AND CAS.[AccountCount] = 1

	UPDATE E -- Override the Account when there is exactly one solution. Otherwise, leave it.
	SET E.[AccountId] = NULL
	FROM @PreprocessedEntries E
	JOIN @PreprocessedLines L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
	LEFT JOIN @ConformantAccounts CA
	ON E.[Index] = CA.[Index] AND E.[LineIndex] = CA.[LineIndex] AND E.[DocumentIndex] = CA.[DocumentIndex]
	WHERE L.[DefinitionId] <> @ManualLineLD
	AND E.[AccountId] = CA.[AccountId]
	AND E.[AccountId] IS NOT NULL AND CA.[AccountId] IS NULL;

	-- We're still assuming that preprocess only modifies, it doesn't insert nor deletes
	SELECT * FROM @PreprocessedDocuments;
	SELECT * FROM @PreprocessedDocumentLineDefinitionEntries;
	SELECT * FROM @PreprocessedLines;
	SELECT * FROM @PreprocessedEntries;
END

	--=-=-=-=-=-=- [C# Preprocessing after SQL], done in api.Documents__Save
	/* 
	
	 [✓] For Smart Lines: If CurrencyId == functional set Value = MonetaryValue
	 [✓] For Manual Lines: If CurrencyId == functional set MonetaryValue = Value

	*/