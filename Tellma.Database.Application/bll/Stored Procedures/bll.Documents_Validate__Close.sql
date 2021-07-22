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
	
	-- Cannot close it if it is not draft
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentIsNotInState0',
		N'localize:Document_State_0'
	FROM @Ids FE
	JOIN [dbo].[Documents] D ON FE.[Id] = D.[Id]
	WHERE D.[State] <> 0;

	-- Cannot close it if it has no attachments
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentHasNoAttachment'
	FROM @Ids FE
	JOIN [dbo].[Documents] D ON FE.[Id] = D.[Id]
	JOIN [dbo].[DocumentDefinitions]  DD ON D.[DefinitionId] = DD.[Id]
	LEFT JOIN [dbo].[Attachments] A ON D.[Id] = A.[DocumentId]
	WHERE DD.[HasAttachments] = 1
	AND A.[Id] IS NULL;

	-- Cannot close a document which does not have lines ready to post
	WITH SatisfactoryDocuments AS (
		SELECT DISTINCT FE.[Index]
		FROM @Ids FE
		JOIN [dbo].[Lines] L ON L.[DocumentId] = FE.[Id]
		JOIN [map].[LineDefinitions]() LD ON L.[DefinitionId] = LD.[Id]
		JOIN [map].[Documents]() D ON FE.[Id] = D.[Id]
		WHERE
			L.[State] = D.[LastLineState]
		OR	LD.[HasWorkflow] = 0 AND L.[State] >= 0
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top) 
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentDoesNotHaveAnyPostedLines'
	FROM @Ids
	WHERE [Index] NOT IN (SELECT [Index] FROM SatisfactoryDocuments);

	-- Cannot close a document which has lines with missing signatures
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasLinesWithMissingSignatures'
	FROM @Ids FE
	JOIN [dbo].[Lines] L ON FE.[Id] = L.[DocumentId]
	JOIN [map].[LineDefinitions]() LD ON L.[DefinitionId] = LD.[Id]
	JOIN [map].[Documents]() D ON FE.[Id] = D.[Id]
	WHERE
			LD.[HasWorkflow] = 1 AND L.[State] BETWEEN 0 AND D.[LastLineState] - 1;

	-- For Relation lines where [BalanceEnforcedState] = 5, the enforcement is at the document closing level
	WITH FE_AB (EntryId, AccountBalanceId) AS (
		SELECT E.[Id] AS EntryId, AB.[Id] AS AccountBalanceId
		FROM [dbo].[Entries] E
		JOIN [dbo].[Lines] L ON E.[LineId] = L.[Id]
		JOIN [map].[LineDefinitions]() LD ON L.[DefinitionId] = LD.[Id]
		JOIN @Ids D ON L.[DocumentId] = D.[Id]
		JOIN [dbo].[AccountBalances] AB ON
			(E.[CenterId] = AB.[CenterId])
		AND (AB.[RelationId] IS NULL OR E.[RelationId] = AB.[RelationId])
		AND (AB.[ResourceId] IS NULL OR E.[ResourceId] = AB.[ResourceId])
		AND (AB.[CurrencyId] = E.[CurrencyId])
		AND (E.[AccountId] = AB.[AccountId])
		WHERE AB.BalanceEnforcedState = 5
		AND (L.[State] = 4 OR LD.[HasWorkflow] = 0 AND L.[State] = 0)
	),
	BreachingEntries ([AccountBalanceId], [NetBalance]) AS (
		SELECT TOP (@Top)
			AB.[Id] AS [AccountBalanceId], 
			FORMAT(SUM(E.[Direction] * E.[MonetaryValue]), 'G', 'en-us') AS NetBalance
		FROM [dbo].[Documents] D
		JOIN [dbo].[Lines] L ON L.DocumentId = D.[Id]
		JOIN [map].[LineDefinitions] () LD ON L.[DefinitionId] = LD.[Id]
		JOIN [dbo].[Entries] E ON L.[Id] = E.[LineId]
		JOIN [dbo].[AccountBalances] AB ON
			(E.[CenterId] = AB.[CenterId])
		AND (AB.[RelationId] IS NULL OR E.[RelationId] = AB.[RelationId])
		AND (AB.[ResourceId] IS NULL OR E.[ResourceId] = AB.[ResourceId])
		AND (AB.[CurrencyId] = E.[CurrencyId])
		AND (E.[AccountId] = AB.[AccountId])
		WHERE AB.[Id] IN (Select AccountBalanceId FROM FE_AB)
		AND ((L.[State] = 4 AND D.[State] = 1) OR 
			(D.[Id] IN (Select [Id] FROM @Ids)) AND 
				(L.[State] = 4 OR LD.HasWorkflow = 0 AND L.[State] = 0))
		GROUP BY AB.[Id], AB.[MinMonetaryBalance], AB.[MaxMonetaryBalance], AB.[MinQuantity], AB.[MaxQuantity]
		HAVING SUM(E.[Direction] * E.[MonetaryValue]) NOT BETWEEN AB.[MinMonetaryBalance] AND AB.[MaxMonetaryBalance]
		OR SUM(E.[Direction] * E.[Quantity]) NOT BETWEEN AB.[MinQuantity] AND AB.[MaxQuantity]
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + '].Lines[' +
			CAST(L.[Index] AS NVARCHAR (255)) + '].Entries[' +
			CAST(E.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheEntryCausesOffLimitBalance0' AS [ErrorName],
		BE.NetBalance
	FROM @Ids D
	JOIN [dbo].[Lines] L ON L.[DocumentId] = D.[Id]
	JOIN [dbo].[Entries] E ON E.[LineId] = L.[Id]
	JOIN FE_AB ON E.[Id] = FE_AB.[EntryId]
	JOIN BreachingEntries BE ON FE_AB.[AccountBalanceId] = BE.[AccountBalanceId];

	-- To do: cannot close a document with a control account having non zero balance
	IF (SELECT [DocumentType] FROM [dbo].[DocumentDefinitions]  WHERE [Id] = @DefinitionId) >= 2 -- N'Event'
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
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS Participant,
		FORMAT(SUM(E.[Direction] * E.[MonetaryValue]), 'G', 'en-us') AS NetBalance
	FROM @Ids D
	JOIN [dbo].[Lines] L ON L.[DocumentId] = D.[Id]
	JOIN [dbo].[Entries] E ON E.[LineId] = L.[Id]
	JOIN [dbo].[Accounts] A ON E.[AccountId] = A.[Id]
	-- TODO: Make the participant required in all control accounts
	LEFT JOIN [dbo].[Relations] R ON E.[NotedRelationId] = R.[Id]
	WHERE A.AccountTypeId IN (SELECT [Id] FROM ControlAccountTypes)
	AND L.[State] >= 0 -- to cater for both Draft in workflow-less and for posted.
	GROUP BY D.[Index], [dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]), E.[CurrencyId], E.[CenterId], [dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) 
	HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0

	-- Verify that workflow-less lines in Events can be in state posted
	INSERT INTO @Documents ([Index], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CenterId], [CenterIsCommon], [RelationId], [RelationIsCommon], [CustodianId], [CustodianIsCommon], [NotedRelationId], [NotedRelationIsCommon],
		[CurrencyId], [CurrencyIsCommon], [ExternalReference], [ExternalReferenceIsCommon],
		[ReferenceSourceId], [ReferenceSourceIsCommon], [InternalReference], [InternalReferenceIsCommon]	
	)
	SELECT Ids.[Index], D.[Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CenterId], [CenterIsCommon], [RelationId], [RelationIsCommon], [CustodianId], [CustodianIsCommon], [NotedRelationId], [NotedRelationIsCommon],
		[CurrencyId], [CurrencyIsCommon], [ExternalReference], [ExternalReferenceIsCommon],
		[ReferenceSourceId], [ReferenceSourceIsCommon], [InternalReference], [InternalReferenceIsCommon]	
	FROM [dbo].[Documents] D JOIN @Ids Ids ON D.[Id] = Ids.[Id]

	INSERT INTO @DocumentLineDefinitionEntries(
		[Index], [DocumentIndex], [Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [RelationId], [RelationIsCommon], [CustodianId], [CustodianIsCommon],
		[NotedRelationId], [NotedRelationIsCommon], [ResourceId], [ResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Time2], [Time2IsCommon], [ExternalReference], [ExternalReferenceIsCommon],
		[ReferenceSourceId], [ReferenceSourceIsCommon], [InternalReference], [InternalReferenceIsCommon])
	SELECT 	DLDE.[Id], Ids.[Index], DLDE.[Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [RelationId], [RelationIsCommon], [CustodianId], [CustodianIsCommon], 
		[NotedRelationId], [NotedRelationIsCommon], [ResourceId], [ResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Time2], [Time2IsCommon], [ExternalReference], [ExternalReferenceIsCommon],
		[ReferenceSourceId], [ReferenceSourceIsCommon], [InternalReference], [InternalReferenceIsCommon]
	FROM DocumentLineDefinitionEntries DLDE
	JOIN @Ids Ids ON DLDE.[DocumentId] = Ids.[Id]
	AND [LineDefinitionId]  IN (SELECT [Id] FROM [map].[LineDefinitions]() WHERE [HasWorkflow] = 0);

	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],	[Memo])
	SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo]
	FROM [dbo].[Lines] L
	JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
	JOIN [map].[Documents]() D ON FE.[Id] = D.[Id]
	WHERE D.[LastLineState] = 4 -- event
	AND L.[DefinitionId] IN (SELECT [Id] FROM [map].[LineDefinitions]() WHERE [HasWorkflow] = 0);
	
	INSERT INTO @Entries (
		[Index], [LineIndex], [DocumentIndex], [Id],
		[Direction], [AccountId], [CurrencyId], [RelationId], [CustodianId], [NotedRelationId], [ResourceId], [CenterId],
		[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [RValue], [PValue], [Time1],
		[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
		[NotedAmount], [NotedDate])
	SELECT
		E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
		E.[Direction],E.[AccountId],E.[CurrencyId], E.[RelationId], E.[CustodianId], E.[NotedRelationId],E.[ResourceId],E.[CenterId],
		E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value], E.[RValue], E.[PValue], E.[Time1],
		E.[Time2],E.[ExternalReference], E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
		E.[NotedAmount],E.[NotedDate]
	FROM [dbo].[Entries] E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__State_Data]
		@Documents = @Documents, @DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @State = 4;

-- Verify that workflow-less lines in Events can be in state authorized
	DELETE FROM @Lines; DELETE FROM @Entries;
	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],		[Memo])
	SELECT	L.[Index],	L.[DocumentId],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo]
	FROM [dbo].[Lines] L
	JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
	JOIN [map].[Documents]() D ON FE.[Id] = D.[Id]
	WHERE D.[LastLineState] = 2 -- template
	AND L.[DefinitionId] IN (SELECT [Id] FROM [map].[LineDefinitions]() WHERE [HasWorkflow] = 0);
	
	INSERT INTO @Entries (
	[Index], [LineIndex], [DocumentIndex], [Id],
	[Direction], [AccountId], [CurrencyId], [RelationId], [CustodianId], [NotedRelationId], [ResourceId], [CenterId],
	[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [Time1],
	[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
	[NotedAmount], [NotedDate])
	SELECT
	E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
	E.[Direction],E.[AccountId],E.[CurrencyId],E.[RelationId], E.[CustodianId],E.[NotedRelationId],E.[ResourceId],E.[CenterId],
	E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value],E.[Time1],
	E.[Time2],E.[ExternalReference],E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
	E.[NotedAmount],E.[NotedDate]
	FROM [dbo].[Entries] E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__State_Data]
		@Documents = @Documents, @DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @State = 2;
				
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;