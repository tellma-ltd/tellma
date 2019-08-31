﻿DECLARE @D11 [dbo].[DocumentList], @L11 [dbo].DocumentLineList, @E11 [dbo].DocumentLineEntryList;
DECLARE @D12 [dbo].[DocumentList], @L12 [dbo].DocumentLineList, @E12 [dbo].DocumentLineEntryList;
DECLARE @D11Ids dbo.[IdList], @D12Ids dbo.[IdList], @D13Ids dbo.[IdList];

BEGIN -- Inserting
	INSERT INTO @D11([Index],
	[DocumentDate],	[Memo], [EvidenceTypeId]) VALUES (
	0, '2017.01.01',		N'Capital investment', N'Attachment'
	);
	INSERT INTO @L11([Index], [DocumentIndex],
				[LineTypeId], [SortKey]) VALUES
		(0,0,	N'ManualLine', 1),
		(1,0,	N'ManualLine', 3),
		(2,0,	N'ManualLine', 4),
		(3,0,	N'ManualLine', 2);

	INSERT INTO @E11 ([Index], [DocumentLineIndex], [DocumentIndex], [EntryNumber],
				[Direction], [AccountId], [IfrsEntryClassificationId],		[ResourceId], [Count], [MoneyAmount],	[Value]) VALUES
		(0,0,0,1,+1,		@CBEUSD,		N'ProceedsFromIssuingShares', 	@USD,			0,			200000,			4700000),
		(1,1,0,1,-1,		@CapitalMA,		N'IssueOfEquity',		@CommonStock,			1000,		0,				2350000),
		(2,2,0,1,-1,		@CapitalAA,		N'IssueOfEquity',		@CommonStock,			1000,		0,				2350000),
		(3,3,0,1,+1,		@CBEUSD,		N'ProceedsFromIssuingShares', 	@USD,			0,			100,			2000);

	EXEC [api].[Documents__Save]
		@DocumentTypeId = N'manual-journals',
		@Documents = @D11, @Lines = @L11, @Entries = @E11,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Insert'
		GOTO Err_Label;
	END;

	INSERT INTO @D11Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D11Ids) ORDER BY [SortKey], [EntryNumber];
END

BEGIN -- Updating document and deleting lines/entries
	INSERT INTO @D12([Id], [DocumentDate],	[Memo], [EvidenceTypeId])
	SELECT [Id], [DocumentDate],	[Memo], [EvidenceTypeId] 
	FROM dbo.Documents
	WHERE [DocumentTypeId] = N'manual-journals' AND [SerialNumber] = 1;

	INSERT INTO @L12([Id], [DocumentId], [DocumentIndex], [LineTypeId], [ScalingFactor], [SortKey])
	SELECT DL.[Id], DL.[DocumentId], D12.[Index], DL.[LineTypeId], [ScalingFactor], [SortKey]
	FROM dbo.DocumentLines DL
	JOIN @D12 D12 ON D12.[Id] = DL.[DocumentId];

	INSERT INTO @E12([Index], [Id], [DocumentLineId], [DocumentIndex], [DocumentLineIndex], [EntryNumber], [Direction], [AccountId], [IfrsEntryClassificationId], [ResourceId], [Count], [MoneyAmount], [Value])
	SELECT ROW_NUMBER() OVER (ORDER BY DLE.[Id]), DLE.[Id], L12.[Id], L12.DocumentIndex, L12.[Index], [EntryNumber], [Direction], [AccountId], [IfrsEntryClassificationId], [ResourceId], [Count], [MoneyAmount], [Value]
	FROM dbo.DocumentLineEntries DLE
	JOIN @L12 L12 ON L12.[Id] = DLE.[DocumentLineId]
	;

	--SELECT * FROM @D12; SELECT * FROM @L12; SELECT * FROM @E12;

	UPDATE @E12 SET [Count] = [Count] / 2, [Value] = [Value] / 2 WHERE [Index] = 1;
	UPDATE @E12 SET [Count] = [Count] * 1.5, [Value] = [Value] * 1.5 + 1175000 WHERE [Index] = 2;
	UPDATE @L12 SET [ScalingFactor] = 3 WHERE [ScalingFactor] = 1;
	DELETE FROM @L12 WHERE [Index] = 1;
	DELETE FROM @L12 WHERE [Index] = 3;

	--SELECT * FROM @D12; SELECT * FROM @L12; SELECT * FROM @E12;

	EXEC [api].[Documents__Save]
		@DocumentTypeId = N'manual-journals',
		@Documents = @D12, @Lines = @L12, @Entries = @E12,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Update and Delete'
		GOTO Err_Label;
	END;

	INSERT INTO @D12Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D12Ids) ORDER BY [SortKey], [EntryNumber];
END

BEGIN -- signing
	DECLARE @DocsToSign [dbo].[IndexedIdList]
	INSERT INTO @DocsToSign([Index], [Id]) SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents;-- WHERE STATE = N'Draft';

	EXEC [api].[Documents__Sign]
		@DocsIndexedIds = @DocsToSign, @ToState = N'Posted', @ReasonDetails = N'seems ok',
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	INSERT INTO @D13Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D13Ids) ORDER BY [SortKey], [EntryNumber];
	SELECT * FROM [rpt].[Documents__Signatures](@D13Ids);

	--select *, ValidFrom AT TIME ZONE 'UTC' AS [SavedAt]  from RoleMemberships;
	--select *, ValidFrom AT TIME ZONE 'UTC' AS [SavedAt] from RoleMembershipsHistory;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Sign'
		GOTO Err_Label;
	END;
END