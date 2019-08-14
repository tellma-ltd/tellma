INSERT INTO @D1(
[DocumentDate],	[Memo], [EvidenceTypeId]) VALUES (
'2017.01.01',		N'Capital investment', N'Attachment'
);
INSERT INTO @L1(
	[LineTypeId], [ScalingFactor]) VALUES
	(N'ManualLine', 1),
	(N'ManualLine', 2);

INSERT INTO @E1 (
	[EntryNumber], [Direction], [AccountId], [IfrsNoteId],				[ResourceId], [Count], [MoneyAmount],	[Value]) VALUES
	(1,				+1,			@CBEUSD,	N'ProceedsFromIssuingShares', 	@USD,		0,			200000,			4700000),
	(2,				-1,			@CapitalMA,N'IssueOfEquity',		@CommonStock,	1000,		0,				2350000),
	(3,				-1,			@CapitalAA,N'IssueOfEquity',		@CommonStock,	1000,		0,				2350000),
	(4,				+1,			@CBEUSD,	N'ProceedsFromIssuingShares', 	@USD,		0,			100,			2000);

EXEC [api].[Documents__Save]
	@DocumentTypeId = N'manual-journals',
	@Documents = @D1, @Lines = @L1, @Entries = @E1,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Capital Investment (M): Insert'
	GOTO Err_Label;
END;

INSERT INTO @D2([Id], [DocumentDate],	[Memo], [EvidenceTypeId])
SELECT [Id], [DocumentDate],	[Memo], [EvidenceTypeId] 
FROM dbo.Documents
WHERE [DocumentTypeId] = N'manual-journals' AND [SerialNumber] = 1;

INSERT INTO @L2([Id], [DocumentId], [DocumentIndex], [LineTypeId], [ScalingFactor])
SELECT DL.[Id], DL.[DocumentId], D2.[Index], DL.[LineTypeId], [ScalingFactor]
FROM dbo.DocumentLines DL
JOIN @D2 D2 ON D2.[Id] = DL.[DocumentId];

INSERT INTO @E2([Id], [DocumentLineId], [DocumentIndex], [DocumentLineIndex], [EntryNumber], [Direction], [AccountId], [IfrsNoteId], [ResourceId], [Count], [MoneyAmount], [Value])
SELECT DLE.[Id], L2.[Id], L2.DocumentIndex, L2.[Index], [EntryNumber], [Direction], [AccountId], [IfrsNoteId], [ResourceId], [Count], [MoneyAmount], [Value]
FROM dbo.DocumentLineEntries DLE
JOIN @L2 L2 ON L2.[Id] = DLE.[DocumentLineId];

--SELECT * FROM dbo.Documents;
--SELECT * FROM dbo.DocumentAssignments;
--SELECT * FROM dbo.DocumentLines; SELECT * FROM dbo.DocumentLineEntries;

UPDATE @E2 SET [Count] = [Count] / 2, [Value] = [Value] / 2 WHERE [Index] = 1;
UPDATE @E2 SET [Count] = [Count] * 1.5, [Value] = [Value] * 1.5 WHERE [Index] = 2;
UPDATE @L2 SET [ScalingFactor] = 3 WHERE [ScalingFactor] = 1;
DELETE FROM @L2 WHERE [Index] = 1;
DELETE FROM @E2 WHERE [Index] = 3;

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

DECLARE @DocsToSign [dbo].[IndexedIdList]
INSERT INTO @DocsToSign([Index], [Id]) SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents;-- WHERE STATE = N'Draft';

EXEC [api].[Documents__Sign]
	@DocsIndexedIds = @DocsToSign, @ToState = N'Posted', @ReasonDetails = N'seems ok',
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

DELETE FROM @DocsIdList ; INSERT INTO @DocsIdList SELECT [Id] FROM dbo.[Documents];
SELECT * FROM [rpt].[Documents__Signatures](@DocsIdList); SELECT * FROM rpt.Documents(@DocsIdList);

select *, ValidFrom AT TIME ZONE 'UTC' AS [SavedAt]  from RoleMemberships;
select *, ValidFrom AT TIME ZONE 'UTC' AS [SavedAt] from RoleMembershipsHistory;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Capital Investment (M): Sign'
	GOTO Err_Label;
END;

--SELECT * FROM dbo.Documents; 
--SELECT * FROM dbo.DocumentLines; 
--SELECT * FROM dbo.DocumentLineEntries;
--SELECT * FROM [DocumentLineEntriesDetailsView];
--SELECT * FROM dbo.DocumentSignatures;