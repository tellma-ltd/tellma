DECLARE @D1 [dbo].[DocumentList], @L1 [dbo].DocumentLineList, @E1 [dbo].DocumentLineEntryList;
DECLARE @D2 [dbo].[DocumentList], @L2 [dbo].DocumentLineList, @E2 [dbo].DocumentLineEntryList;
DECLARE @D1Ids dbo.[IdList], @D2Ids dbo.[IdList], @D3Ids dbo.[IdList];

BEGIN -- Inserting
	INSERT INTO @D1(
	[DocumentDate],	[Memo], [EvidenceTypeId]) VALUES (
	'2017.01.01',		N'Capital investment', N'Attachment'
	);
	INSERT INTO @L1(
		[DocumentIndex], [LineTypeId], [SortKey]) VALUES
		(0,					N'ManualLine', 1),
		(0,					N'ManualLine', 3),
		(0,					N'ManualLine', 4),
		(0,					N'ManualLine', 2);

	INSERT INTO @E1 (
		[DocumentIndex], [DocumentLineIndex], [Direction], [AccountId], [IfrsNoteId],				[ResourceId], [Quantity], [Count], [MoneyAmount],	[Value]) VALUES
		(0,					0,					+1,			@CBEUSD,	N'ProceedsFromIssuingShares', 	@USD,		200000,		0,			200000,			4700000),
		(0,					1,					-1,			@CapitalMA,N'IssueOfEquity',		@CommonStock,		1000,		1000,			0,				2350000),
		(0,					2,					-1,			@CapitalAA,N'IssueOfEquity',		@CommonStock,		1000,		1000,			0,				2350000),
		(0,					3,					+1,			@CBEUSD,	N'ProceedsFromIssuingShares', 	@USD,		100,		0,			100,			2000);

	EXEC [api].[Documents__Save]
		@DocumentTypeId = N'manual-journals',
		@Documents = @D1, @Lines = @L1, @Entries = @E1,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Insert'
		GOTO Err_Label;
	END;

	INSERT INTO @D1Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D1Ids) ORDER BY [SortKey], [EntryNumber];
END

BEGIN -- Updating document and deleting lines/entries
	INSERT INTO @D2([Id], [DocumentDate],	[Memo], [EvidenceTypeId])
	SELECT [Id], [DocumentDate],	[Memo], [EvidenceTypeId] 
	FROM dbo.Documents
	WHERE [DocumentTypeId] = N'manual-journals' AND [SerialNumber] = 1;

	INSERT INTO @L2([Id], [DocumentId], [DocumentIndex], [LineTypeId], [ScalingFactor], [SortKey])
	SELECT DL.[Id], DL.[DocumentId], D2.[Index], DL.[LineTypeId], [ScalingFactor], [SortKey]
	FROM dbo.DocumentLines DL
	JOIN @D2 D2 ON D2.[Id] = DL.[DocumentId];

	INSERT INTO @E2([Id], [DocumentLineId], [DocumentIndex], [DocumentLineIndex], [EntryNumber], [Direction], [AccountId], [IfrsNoteId], [ResourceId], [Quantity], [Count], [MoneyAmount], [Value])
	SELECT DLE.[Id], L2.[Id], L2.DocumentIndex, L2.[Index], [EntryNumber], [Direction], [AccountId], [IfrsNoteId], [ResourceId], [Quantity], [Count], [MoneyAmount], [Value]
	FROM dbo.DocumentLineEntries DLE
	JOIN @L2 L2 ON L2.[Id] = DLE.[DocumentLineId];

	--SELECT * FROM @D2; SELECT * FROM @L2; SELECT * FROM @E2;

	UPDATE @E2 SET [Quantity] = [Quantity]/2, [Count] = [Count] / 2, [Value] = [Value] / 2 WHERE [Index] = 1;
	UPDATE @E2 SET [Quantity] = [Quantity] * 1.5, [Count] = [Count] * 1.5, [Value] = [Value] * 1.5 + 1175000 WHERE [Index] = 2;
	UPDATE @L2 SET [ScalingFactor] = 3 WHERE [ScalingFactor] = 1;
	DELETE FROM @L2 WHERE [Index] = 1;
	DELETE FROM @L2 WHERE [Index] = 3;

	--SELECT * FROM @D2; SELECT * FROM @L2; SELECT * FROM @E2;

	EXEC [api].[Documents__Save]
		@DocumentTypeId = N'manual-journals',
		@Documents = @D2, @Lines = @L2, @Entries = @E2,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Update and Delete'
		GOTO Err_Label;
	END;

	INSERT INTO @D2Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D2Ids) ORDER BY [SortKey], [EntryNumber];
END

BEGIN -- signing
	DECLARE @DocsToSign [dbo].[IndexedIdList]
	INSERT INTO @DocsToSign([Index], [Id]) SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents;-- WHERE STATE = N'Draft';

	EXEC [api].[Documents__Sign]
		@DocsIndexedIds = @DocsToSign, @ToState = N'Posted', @ReasonDetails = N'seems ok',
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	INSERT INTO @D3Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D3Ids) ORDER BY [SortKey], [EntryNumber];
	SELECT * FROM [rpt].[Documents__Signatures](@D3Ids);

	--select *, ValidFrom AT TIME ZONE 'UTC' AS [SavedAt]  from RoleMemberships;
	--select *, ValidFrom AT TIME ZONE 'UTC' AS [SavedAt] from RoleMembershipsHistory;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Sign'
		GOTO Err_Label;
	END;
END
