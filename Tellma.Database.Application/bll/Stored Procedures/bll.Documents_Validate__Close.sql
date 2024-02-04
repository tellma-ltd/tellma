CREATE PROCEDURE [bll].[Documents_Validate__Close]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @Documents [dbo].[DocumentList], @DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList],
			@Lines [dbo].[LineList], @Entries [dbo].[EntryList];
	DECLARE @ManualJV INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ManualJournalVoucher');
	SET @IsError = 0;
	-- cannot close if the line posting date falls in an archived period. Logic repeated at line level
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_FallsinArchivedPeriod', NULL
	FROM @Ids FE
	JOIN dbo.Documents D ON FE.[Id] = D.[Id]
	JOIN dbo.Lines L ON L.[DocumentId] = D.[Id]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	WHERE L.[PostingDate] <= (SELECT [ArchiveDate] FROM dbo.Settings)
	AND LD.[LineType] >= 100
	UNION
	-- Cannot close it if it is not draft
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentIsNotInState0',
		N'localize:Document_State_0'
	FROM @Ids FE
	JOIN [dbo].[Documents] D ON FE.[Id] = D.[Id]
	WHERE D.[State] <> 0
	UNION
	-- Cannot close it if it has no attachments while attachments are required
	--INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentHasNoAttachment', NULL
	FROM @Ids FE
	JOIN [dbo].[Documents] D ON FE.[Id] = D.[Id]
	JOIN [dbo].[DocumentDefinitions]  DD ON D.[DefinitionId] = DD.[Id]
	LEFT JOIN [dbo].[Attachments] A ON D.[Id] = A.[DocumentId]
	WHERE DD.[AttachmentVisibility] = N'Required'
	AND A.[Id] IS NULL;

	-- Cannot close a document where there are no lines, or where all lines have negative state
	-- So, we take all documents and remove from them those with positive states
	WITH NonSatisfactoryDocuments AS (
		SELECT [Index]
		FROM @Ids
		EXCEPT (
			SELECT DISTINCT FE.[Index]
			FROM @Ids FE
			JOIN [dbo].[Lines] L ON L.[DocumentId] = FE.[Id]
			JOIN [map].[LineDefinitions]() LD ON L.[DefinitionId] = LD.[Id]
			WHERE
				LD.[HasWorkflow] = 1 AND L.[State]  = LD.[LastLineState]
			OR	LD.[HasWorkflow] = 0
		)
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top) 
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentDoesNotHaveAnyPostedLines'
	FROM @Ids
	WHERE [Index] IN (SELECT [Index] FROM NonSatisfactoryDocuments);

	-- Cannot close a document which has lines with missing signatures
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasLinesWithMissingSignatures'
	FROM @Ids FE
	JOIN [dbo].[Lines] L ON L.[DocumentId] = FE.[Id]
	JOIN [map].[LineDefinitions]() LD ON LD.[Id] = L.[DefinitionId]
	WHERE LD.[HasWorkflow] = 1 AND L.[State] BETWEEN 0 AND LD.[LastLineState] - 1;

	-- To do: cannot close a document with a control account having non zero balance
	IF (@DefinitionId <> @ManualJV)
	AND EXISTS (
		SELECT * FROM
		dbo.DocumentDefinitionLineDefinitions DDLD
		JOIN dbo.LineDefinitions LD ON LD.[Id] = DDLD.[LineDefinitionId]
		WHERE DDLD.[DocumentDefinitionId] = @DefinitionId
		AND LD.[LineType] >= 100 -- N'Event', N'Regulatory'
	)
	WITH ControlAccountTypes AS (
		SELECT [Id]
		FROM [dbo].[AccountTypes]
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM [dbo].[AccountTypes] WHERE [Concept] = N'ControlAccountsExtension')
		) = 1
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasControlAccount0For1WithNetBalance2' AS [ErrorName],
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) As AccountName,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS NotedAgent,
		FORMAT(SUM(E.[Direction] * E.[MonetaryValue]), 'G', 'en-us') AS NetBalance
	FROM @Ids D
	JOIN [dbo].[Lines] L ON L.[DocumentId] = D.[Id]
	JOIN [dbo].[LineDefinitions] LD ON LD.[Id] = L.[DefinitionId]
	JOIN [dbo].[Entries] E ON E.[LineId] = L.[Id]
	JOIN [dbo].[Accounts] A ON E.[AccountId] = A.[Id]
	-- MA: LEFT JOIN => JOIN, assuming control accounts have Noted Agent. 2021.12.11
	JOIN [dbo].[Agents] R ON E.[NotedAgentId] = R.[Id]
	WHERE A.AccountTypeId IN (SELECT [Id] FROM ControlAccountTypes)
	AND LD.[LineType] >= 100 -- N'Event', N'Regulatory'
	AND L.[State] >= 0 -- to cater for both Draft in workflow-less and for posted.
	GROUP BY D.[Index], [dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]), E.[CurrencyId], E.[CenterId], [dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) 
	HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	UNION
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasControlAccount0For1WithNetBalance2' AS [ErrorName],
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]) As AccountName,
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS NotedAgent,
		FORMAT(SUM(E.[Direction] * E.[Value]), 'G', 'en-us') AS NetBalance
	FROM @Ids D
	JOIN [dbo].[Lines] L ON L.[DocumentId] = D.[Id]
	JOIN [dbo].[LineDefinitions] LD ON LD.[Id] = L.[DefinitionId]
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	-- MA: LEFT JOIN => JOIN, assuming control accounts have Noted Agent. 2021.12.11
	JOIN [dbo].[Agents] R ON E.[NotedAgentId] = R.[Id]
	WHERE A.AccountTypeId IN (SELECT [Id] FROM ControlAccountTypes)
	AND LD.[LineType] >= 100
	AND L.[State] >= 0 -- to cater for both Draft in workflow-less and for posted.
	-- MA: removed CurrencyId From GROUP BY, 2021.12.11
	GROUP BY D.[Index], [dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]), E.[CenterId], [dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) 
	HAVING SUM(E.[Direction] * E.[Value]) <> 0

	-- cannot close a document with sales invoice, if it violates one of the following
	DECLARE @Country NCHAR (2) = dal.fn_Settings__Country();
	IF @Country = N'SA' AND @DefinitionId <> @ManualJV
	AND EXISTS(
		SELECT *
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN @Ids D ON D.[Id] = L.[DocumentId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE AC.[Concept] = N'CurrentValueAddedTaxPayables'
	)
	BEGIN
		PRINT N'Validate all Zatca rules'
	END

	IF EXISTS(SELECT * FROM @ValidationErrors) GOTO DONE;
	-- Verify that workflow-less lines in Documents can be in their final state
	INSERT INTO @Documents ([Index], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]	
	)
	SELECT Ids.[Index], D.[Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]
	FROM [dbo].[Documents] D JOIN @Ids Ids ON D.[Id] = Ids.[Id]

	INSERT INTO @DocumentLineDefinitionEntries(
		[Index], [DocumentIndex], [Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]
	)
	SELECT 	DLDE.[Id], Ids.[Index], DLDE.[Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]
	FROM DocumentLineDefinitionEntries DLDE
	JOIN @Ids Ids ON DLDE.[DocumentId] = Ids.[Id]
	AND [LineDefinitionId]  IN (SELECT [Id] FROM [map].[LineDefinitions]() WHERE [HasWorkflow] = 0);

	-- Verify that lines whose last state = approved meet the conditions to be approved
	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],	[Memo],
			[Decimal1], [Decimal2], [Boolean1], [Text1], [Text2])
	SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo],
			L.[Decimal1], L.[Decimal2], L.[Boolean1], L.[Text1], L.[Text2]
	FROM [dbo].[Lines] L
	JOIN map.LineDefinitions() LD ON LD.[Id] = L.[DefinitionId]
	JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
	JOIN [map].[Documents]() D ON FE.[Id] = D.[Id]
	WHERE LD.[LastLineState] = 2
	
	INSERT INTO @Entries (
		[Index], [LineIndex], [DocumentIndex], [Id],
		[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId], [CenterId],
		[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [RValue], [PValue], [Time1],
		[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
		[NotedAmount], [NotedDate])
	SELECT
		E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
		E.[Direction],E.[AccountId],E.[CurrencyId], E.[AgentId], E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId], E.[CenterId],
		E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value], E.[RValue], E.[PValue], E.[Time1],
		E.[Time2],E.[ExternalReference], E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
		E.[NotedAmount],E.[NotedDate]
	FROM [dbo].[Entries] E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	IF EXISTS(SELECT * FROM @Lines)
--	INSERT INTO @ValidationErrors -- to avoid NESTED INSERT EXEC
	EXEC [bll].[Lines_Validate__Transition_ToState]
		@Documents = @Documents, 
		@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @ToState = 2, 
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
	IF @IsError = 1 RETURN; -- to avoid NESTED INSERT EXEC

	IF EXISTS(SELECT * FROM @Lines)
	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__State_Data]
		@Documents = @Documents, @DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @State = 2,
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
	IF @IsError = 1 GOTO DONE;

	DELETE FROM @Lines; DELETE FROM @Entries;
	-- Verify that lines whose last state = posted meet the conditions to be posted
	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],	[Memo],
			[Decimal1], [Decimal2], [Boolean1], [Text1], [Text2])
	SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo],
			L.[Decimal1], L.[Decimal2], L.[Boolean1], L.[Text1], L.[Text2]
	FROM [dbo].[Lines] L
	JOIN map.LineDefinitions() LD ON LD.[Id] = L.[DefinitionId]
	JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
	JOIN [map].[Documents]() D ON FE.[Id] = D.[Id]
	WHERE LD.[LastLineState] = 4

	INSERT INTO @Entries (
		[Index], [LineIndex], [DocumentIndex], [Id],
		[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId], [CenterId],
		[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [RValue], [PValue], [Time1],
		[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
		[NotedAmount], [NotedDate])
	SELECT
		E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
		E.[Direction],E.[AccountId],E.[CurrencyId], E.[AgentId], E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId], E.[CenterId],
		E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value], E.[RValue], E.[PValue], E.[Time1],
		E.[Time2],E.[ExternalReference], E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
		E.[NotedAmount],E.[NotedDate]
	FROM [dbo].[Entries] E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	IF EXISTS(SELECT * FROM @Lines)
--	INSERT INTO @ValidationErrors -- to avoid NESTED INSERT EXEC
	EXEC [bll].[Lines_Validate__Transition_ToState]
		@Documents = @Documents, 
		@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @ToState = 4, 
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
	IF @IsError = 1 RETURN; -- to avoid NESTED INSERT EXEC

	IF EXISTS(SELECT * FROM @Lines)
	INSERT INTO @ValidationErrors -- to avoid NESTED INSERT EXEC
	EXEC [bll].[Lines_Validate__State_Data]
		@Documents = @Documents, @DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @State = 4,
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
	IF @IsError = 1 GOTO DONE;

	DECLARE @CloseValidateScript NVARCHAR (MAX) = (SELECT [CloseValidateScript] FROM dbo.DocumentDefinitions WHERE [Id] = @DefinitionId);
	IF @CloseValidateScript IS NOT NULL
	BEGIN TRY
		DELETE FROM @Lines; DELETE FROM @Entries;
		-- Pass @Lines and @Entries to the vlidate script
		INSERT INTO @Lines(
				[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],	[Memo],
				[Decimal1], [Decimal2], [Boolean1], [Text1], [Text2])
		SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo],
				L.[Decimal1], L.[Decimal2], L.[Boolean1], L.[Text1], L.[Text2]
		FROM [dbo].[Lines] L
		JOIN map.LineDefinitions() LD ON LD.[Id] = L.[DefinitionId]
		JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
		JOIN [map].[Documents]() D ON FE.[Id] = D.[Id]

		INSERT INTO @Entries (
			[Index], [LineIndex], [DocumentIndex], [Id],
			[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId], [CenterId],
			[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [RValue], [PValue], [Time1],
			[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
			[NotedAmount], [NotedDate])
		SELECT
			E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
			E.[Direction],E.[AccountId],E.[CurrencyId], E.[AgentId], E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId], E.[CenterId],
			E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value], E.[RValue], E.[PValue], E.[Time1],
			E.[Time2],E.[ExternalReference], E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
			E.[NotedAmount],E.[NotedDate]
		FROM [dbo].[Entries] E
		JOIN @Lines L ON E.[LineId] = L.[Id];

		INSERT INTO @ValidationErrors
		EXECUTE	dbo.sp_executesql @CloseValidateScript, N'
			@DefinitionId INT,
			@Documents [dbo].[DocumentList] READONLY,
			@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
			@Lines [dbo].[LineList] READONLY, 
			@Entries [dbo].EntryList READONLY,
			@Top INT', 	@DefinitionId = @DefinitionId, @Documents = @Documents,
			@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries, @Lines = @Lines, @Entries = @Entries, @Top = @Top;
	END TRY
	BEGIN CATCH
		DECLARE @ErrorNumber INT = 100000 + ERROR_NUMBER();
		DECLARE @ErrorMessage NVARCHAR (255) = ERROR_MESSAGE();
		DECLARE @ErrorState TINYINT = 99;
		THROW @ErrorNumber, @ErrorMessage, @ErrorState;
	END CATCH
DONE:
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;
	SELECT TOP (@Top) * FROM @ValidationErrors;
END;
GO