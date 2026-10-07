CREATE PROCEDURE [bll].[Documents_Validate__Open]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @Documents DocumentList, @DocumentLineDefinitionEntries DocumentLineDefinitionEntryList,
			@Lines LineList, @Entries EntryList;

    -- Non Null Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentWithId0WasNotFound',
		CAST([Id] AS NVARCHAR (255))
    FROM @Ids
    WHERE Id <> 0
	AND Id NOT IN (SELECT Id from [dbo].[Documents]);

	IF EXISTS(SELECT * FROM @ValidationErrors) GOTO DONE

	-- Cannot unpost it if it is not posted
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentIsNotInState0',
		N'localize:Document_State_1'
	FROM @Ids FE
	JOIN dbo.Documents D ON FE.[Id] = D.[Id]
	WHERE D.[State] <> 1;	

	-- Cannot unpost it if it is a Zatca document
	IF (SELECT [ZatcaEnvironment] FROM dbo.Settings) <> N'Sandbox'
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotOpenAZatcaDocument',
		D.[Code]
	FROM @Ids FE
	JOIN map.Documents() D ON FE.[Id] = D.[Id]
	WHERE D.[ZatcaState] = 10	

	-- Cannot unpost it if it is, or may be, on the Marmin (UAE) Peppol network:
	--    1  Submitted             the vendor accepted it; Peppol validation in progress
	--    2  SentAwaitingOutcome   the request left and its outcome is unknown, so it may have landed
	--   10  Delivered
	--  -30  PeppolRejected        delivered and then rejected; the remedy is a credit note
	--
	-- Stricter than the ZATCA rule above, which only refuses once reported. These stay
	-- reopenable, because none of them is on the network and each has a remedy that starts with
	-- fixing the document: NULL (never submitted), 0 (claimed, never sent), -10 (refused by the
	-- vendor) and -20 (failed Peppol validation; a re-close resubmits it).
	--
	-- This single guard also covers delete and cancel: bll.Documents_Validate__Delete refuses a
	-- closed document, and bll.Documents_Validate__Cancel requires State = 0, so both already
	-- require reopening first.
	--
	-- Sandbox is deliberately exempt so the integration can be exercised repeatedly. Instead,
	-- DocumentsService calls dal.MarminAe__ResetOnOpen, which clears the Marmin columns of a
	-- reopened sandbox document so that stale vendor state cannot outlive the content it described.
	IF (SELECT [MarminAeEnvironment] FROM dbo.Settings) <> N'Sandbox'
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotOpenAMarminAeDocument',
		D.[Code]
	FROM @Ids FE
	JOIN map.Documents() D ON FE.[Id] = D.[Id]
	WHERE D.[MarminAeState] >= 1 OR D.[MarminAeState] = -30

	-- [C#] cannot open if the document posting date falls in an archived period.
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].PostingDate',
		N'Error_FallsinArchivedPeriod'
	FROM @Ids FE
	JOIN dbo.Lines L ON L.[DocumentId] = FE.[Id]
	WHERE L.[PostingDate] <= (SELECT [ArchiveDate] FROM dbo.Settings)
	AND L.[State] > 0;

	-- Actually, we might want to allow opening 
	-- [C#] cannot open if the document posting date falls in a frozen period.
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].PostingDate',
		N'Error_FallsinFrozenPeriod'
	FROM @Ids FE
	JOIN dbo.Lines L ON L.[DocumentId] = FE.[Id]
	WHERE L.[PostingDate] <= (SELECT [FreezeDate] FROM dbo.Settings)
	AND L.[State] > 0;

	INSERT INTO @Documents ([Index], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [ResourceId], [ResourceIsCommon],
		[NotedAgentId], [NotedAgentIsCommon],[NotedResourceId], [NotedResourceIsCommon],
		[CurrencyId], [CurrencyIsCommon], [ExternalReference], [ExternalReferenceIsCommon], 
		[ReferenceSourceId], [ReferenceSourceIsCommon], [InternalReference], [InternalReferenceIsCommon]
	)
	SELECT [Id], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon],  [ResourceId], [ResourceIsCommon],
		[NotedAgentId], [NotedAgentIsCommon],[NotedResourceId], [NotedResourceIsCommon],
		[CurrencyId], [CurrencyIsCommon], [ExternalReference], [ExternalReferenceIsCommon],
		[ReferenceSourceId], [ReferenceSourceIsCommon], [InternalReference], [InternalReferenceIsCommon]
	FROM dbo.Documents
	WHERE [Id] IN (SELECT [Id] FROM @Ids)

	INSERT INTO @DocumentLineDefinitionEntries(
		[Index], [DocumentIndex], [Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon],
		[NotedAgentId], [NotedAgentIsCommon], [ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon],
		[Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Time2], [Time2IsCommon], [ExternalReference], [ExternalReferenceIsCommon], [InternalReference],
		[InternalReferenceIsCommon])
	SELECT 		[Id], [DocumentId], [Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon],
		[NotedAgentId], [NotedAgentIsCommon], [ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon],
		[Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Time2], [Time2IsCommon], [ExternalReference], [ExternalReferenceIsCommon], [InternalReference],
		[InternalReferenceIsCommon]
	FROM DocumentLineDefinitionEntries
	WHERE [DocumentId] IN (SELECT [Id] FROM @Ids)
	AND [LineDefinitionId]  IN (SELECT [Id] FROM map.LineDefinitions() WHERE [HasWorkflow] = 0);

	-- Verify that workflow-less lines in Events can be in state draft
	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],		[Memo])
	SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo]
	FROM dbo.Lines L
	JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
	AND L.[DefinitionId] IN (SELECT [Id] FROM map.LineDefinitions() WHERE [HasWorkflow] = 0);
	
	INSERT INTO @Entries (
	[Index], [LineIndex], [DocumentIndex], [Id],
	[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId],
	[CenterId],	[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [Time1],
	[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
	[NotedAmount], [NotedDate])
	SELECT
	E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
	E.[Direction],E.[AccountId],E.[CurrencyId],E.[AgentId],E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId],
	E.[CenterId], E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value],E.[Time1],
	E.[Time2],E.[ExternalReference],E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
	E.[NotedAmount],E.[NotedDate]
	FROM dbo.Entries E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__State_Data]
		@Documents = @Documents, 
		@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, 
		@Entries = @Entries, 
		@State = 0, 
		@Top = @Top, 
		@IsError = @IsError OUTPUT;

DONE:
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;
GO
