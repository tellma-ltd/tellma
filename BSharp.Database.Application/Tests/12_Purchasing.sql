DECLARE @D4 [dbo].[DocumentList], @L4 [dbo].DocumentLineList, @E4 [dbo].DocumentLineEntryList;
DECLARE @D5 [dbo].[DocumentList], @L5 [dbo].DocumentLineList, @E5 [dbo].DocumentLineEntryList;
DECLARE @D4Ids dbo.[IdList], @D5Ids dbo.[IdList], @D6Ids dbo.[IdList];

BEGIN -- Inserting
	INSERT INTO @D4(
	[DocumentDate],	[Memo], [EvidenceTypeId]) VALUES (
	'2017.01.05',		N'Purchase of Hot Roll coils', N'Attachment'
	);
	INSERT INTO @L4( --DocumentIndex DEFAULT 0
		[LineTypeId], [SortKey]) VALUES
		(N'GoodReceiptInTransitWithInvoice', 1),
		(N'GoodReceiptInTransitWithInvoice', 2);

	INSERT INTO @E4 (--DocumentIndex DEFAULT 0
[DocumentLineIndex], [Direction], [AccountId], [IfrsNoteId],								[ResourceId], [MoneyAmount],	[Mass], [Value]) VALUES
		(0,					+1,			@ESL,	N'purchase', 								@HR1000x1_9,	0,			500000, 4846800),
		(0,					-1,			@CBELC,	N'PaymentsToSuppliersForGoodsAndServices',	@ETB,			4846800,		0,	4846800),
		(1,					-1,			@ESL	,N'purchase',								@CR1000x1_4,	0,			500000,	8078000),
		(1,					+1,			@CBELC,	N'PaymentsToSuppliersForGoodsAndServices', 	@ETB,			8078000,		0,	8078000);

	EXEC [api].[Documents__Save]
		@DocumentTypeId = N'purchasing-international',
		@Documents = @D4, @Lines = @L4, @Entries = @E4,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Purchasing: Insert'
		GOTO Err_Label;
	END;

	INSERT INTO @D4Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D4Ids) ORDER BY [SortKey], [EntryNumber];
END

/*
BEGIN -- Updating document and deleting lines/entries
	INSERT INTO @D5([Id], [DocumentDate],	[Memo], [EvidenceTypeId])
	SELECT [Id], [DocumentDate],	[Memo], [EvidenceTypeId] 
	FROM dbo.Documents
	WHERE [DocumentTypeId] = N'purchasing-international' AND [SerialNumber] = 1;

	INSERT INTO @L5([Id], [DocumentId], [DocumentIndex], [LineTypeId], [ScalingFactor], [SortKey])
	SELECT DL.[Id], DL.[DocumentId], D5.[Index], DL.[LineTypeId], [ScalingFactor], [SortKey]
	FROM dbo.DocumentLines DL
	JOIN @D5 D5 ON D5.[Id] = DL.[DocumentId];

	INSERT INTO @E5([Id], [DocumentLineId], [DocumentIndex], [DocumentLineIndex], [EntryNumber], [Direction], [AccountId], [IfrsNoteId], [ResourceId], [Count], [MoneyAmount], [Value])
	SELECT DLE.[Id], L5.[Id], L5.DocumentIndex, L5.[Index], [EntryNumber], [Direction], [AccountId], [IfrsNoteId], [ResourceId], [Count], [MoneyAmount], [Value]
	FROM dbo.DocumentLineEntries DLE
	JOIN @L5 L5 ON L5.[Id] = DLE.[DocumentLineId];

	--SELECT * FROM @D5; SELECT * FROM @L5; SELECT * FROM @E5;

	UPDATE @E5 SET [Count] = [Count] / 2, [Value] = [Value] / 2 WHERE [Index] = 1;
	UPDATE @E5 SET [Count] = [Count] * 1.5, [Value] = [Value] * 1.5 + 1175000 WHERE [Index] = 2;
	UPDATE @L5 SET [ScalingFactor] = 3 WHERE [ScalingFactor] = 1;
	DELETE FROM @L5 WHERE [Index] = 1;
	DELETE FROM @L5 WHERE [Index] = 3;

	--SELECT * FROM @D5; SELECT * FROM @L5; SELECT * FROM @E5;

	EXEC [api].[Documents__Save]
		@DocumentTypeId = N'purchasing-international',
		@Documents = @D5, @Lines = @L5, @Entries = @E5,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Update and Delete'
		GOTO Err_Label;
	END;

	INSERT INTO @D5Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D5Ids) ORDER BY [SortKey], [EntryNumber];
END

BEGIN -- signing
	DECLARE @DocsToSign [dbo].[IndexedIdList]
	INSERT INTO @DocsToSign([Index], [Id]) SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents;-- WHERE STATE = N'Draft';

	EXEC [api].[Documents__Sign]
		@DocsIndexedIds = @DocsToSign, @ToState = N'Posted', @ReasonDetails = N'seems ok',
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	INSERT INTO @D6Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D6Ids) ORDER BY [SortKey], [EntryNumber];
	SELECT * FROM [rpt].[Documents__Signatures](@D6Ids);

	--select *, ValidFrom AT TIME ZONE 'UTC' AS [SavedAt]  from RoleMemberships;
	--select *, ValidFrom AT TIME ZONE 'UTC' AS [SavedAt] from RoleMembershipsHistory;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Sign'
		GOTO Err_Label;
	END;
END
*/