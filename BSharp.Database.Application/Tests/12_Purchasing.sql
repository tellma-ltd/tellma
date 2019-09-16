DECLARE @D21 [dbo].[DocumentList], @L21 [dbo].DocumentLineList, @E21 [dbo].DocumentLineEntryList;
DECLARE @D22 [dbo].[DocumentList], @L22 [dbo].DocumentLineList, @E22 [dbo].DocumentLineEntryList;
DECLARE @D21Ids dbo.[IdList], @D22Ids dbo.[IdList], @D23Ids dbo.[IdList];

BEGIN -- Inserting
	INSERT INTO @D21 ([Index],
			[DocumentDate],	[Memo], [EvidenceTypeId]) VALUES
		(0,	'2017.01.05',	N'Purchase of hot and cold coils', N'Attachment');
	INSERT INTO @L21([Index], [DocumentIndex],
				[LineTypeId],				[SortKey]) VALUES
		(0,0,	N'GoodReceiptWithInvoice',	1),
		(1,0,	N'GoodReceiptWithInvoice',	2),
		(2,0,	N'CashIssue',				1);
	INSERT INTO @E21 ([Index], [DocumentLineIndex], [DocumentIndex], [EntryNumber],
					[Direction], [AccountId], [IfrsEntryClassificationId],			[ResourceId], [MonetaryValue],[Mass], [Value]) VALUES
		(0,0,0,1,	+1,			@ESL,			N'InventoryPurchaseExtension', 		@HR1000x1_9,	0,			500000, 0),
		(1,0,0,2,	-1,			@VimeksAccount,	NULL,								@USD,		100000,			0,		0),
		(2,1,0,1,	+1,			@ESL,			N'InventoryPurchaseExtension',		@CR1000x1_4,	0,			500000, 0),
		(3,1,0,2,	-1,			@VimeksAccount,	NULL,								@USD,		200000,			0,		0),
		(4,2,0,1,	+1,			@VimeksAccount,	NULL,								@USD,		300000,			0,		0),
		(5,2,0,2,	-1,			@CBELC,			N'PaymentsToSuppliersForGoodsAndServices',@ETB,	9000000,		0,		0);

	INSERT INTO @D21 ([Index],
			[DocumentDate],	[Memo], [EvidenceTypeId]) VALUES
		(1,	'2017.01.06',	N'Purchase of fuel On Credit', N'Attachment');
	INSERT INTO @L21([Index], [DocumentIndex],
				[LineTypeId],				[SortKey]) VALUES
		(3,1,	 N'GoodReceiptWithInvoice', 1),
		(4,1,	 N'GoodReceiptWithInvoice', 2);

	INSERT INTO @E21 ([Index], [DocumentLineIndex], [DocumentIndex], [EntryNumber],
					[Direction], [AccountId], [IfrsEntryClassificationId],			[ResourceId], [MonetaryValue],[Volume], [Value]) VALUES
		(6,3,1,1,	+1,			@fuelHR,		N'TransportationExpense', 			@Oil,			0,			20,		0),
		(7,3,1,2,	-1,			@NocJimmaAccount,	NULL,							@ETB,		430.6,			0,		0),
		(8,4,1,1,	+1,			@fuelHR,		N'TransportationExpense',			@Diesel,	0,				30,		0),
		(9,4,1,2,	-1,			@NocJimmaAccount,	NULL,							@ETB,		562.5,			0,		0);

	--INSERT INTO @D21
	--([DocumentDate],	[Memo], [EvidenceTypeId]) VALUES
	--('2017.01.07',		N'Purchase of fixed assets by check and cash', N'Attachment');
	--INSERT INTO @L21(
	--[DocumentIndex], [LineTypeId], [SortKey]) VALUES
	--		(2, N'GoodReceiptInWithInvoiceWithVAT', 1),
	--		(2, N'CashIssue', 1),
	--		(2, N'CashIssue', 2);
	

	EXEC [api].[Documents__Save]
		@DocumentTypeId = N'purchasing-international',
		@Documents = @D21, @Lines = @L21, @Entries = @E21,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Purchasing: Insert'
		GOTO Err_Label;
	END;

	INSERT INTO @D21Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D21Ids) ORDER BY [SortKey], [EntryNumber];

	WITH GritwiLines AS (
		SELECT
			SortKey, LineId, LineTypeId, EntryNumber, Account, IfrsEntryClassificationId, [Resource], Direction, Quantity, [Unit], [Value]
		FROM rpt.Documents(@D21Ids)
		WHERE LineTypeId = N'GoodReceiptInWithInvoice'
	)
	SELECT L1.Account As LC, L1.[Resource] As [Item], L1.[Quantity], L1.[Unit], L2.[Quantity] As [Price], L2.[Resource] AS [Currency], L2.[Account] AS [Supplier]
	FROM (
		SELECT * FROM GritwiLines WHERE EntryNumber = 1
	) L1 JOIN (
		SELECT * FROM GritwiLines WHERE EntryNumber = 2
	) L2 ON L2.LineId = L1.LineId;

	WITH CompactLines AS (
		SELECT [AccountId], [IfrsEntryClassificationId], [ResourceId],
			SUM([Direction] * [MonetaryValue]) AS [MonetaryValue],
			SUM([Direction] * [Mass]) AS [Mass],
			SUM([Direction] * [Value]) AS [Value]
		FROM DocumentLineEntries
		GROUP BY [AccountId], [IfrsEntryClassificationId], [ResourceId]
	)
	SELECT A.[Name] AS [Account], IEC.Label AS [Note], R.[Name] AS [Resource],
	MUM.[Name] AS [MassUnit], MUV.[Name] AS [VolumeUnit],
	CAST([MonetaryValue] AS MONEY) AS [MoneyAmount],
	CAST([Mass] AS MONEY) AS [Mass],
	CAST([Value] AS MONEY) AS [Value]
	FROM CompactLines CL
	LEFT JOIN dbo.Resources R ON CL.[ResourceId] = R.[Id]
	LEFT JOIN dbo.MeasurementUnits MUM ON R.MassUnitId = MUM.Id
	LEFT JOIN dbo.MeasurementUnits MUV ON R.VolumeUnitId = MUV.Id
	JOIN dbo.Accounts A ON CL.[AccountId] = A.[Id]
	JOIN dbo.IfrsEntryClassifications IEC ON CL.IfrsEntryClassificationId = IEC.Id
	WHERE CL.[MonetaryValue] <> 0 OR CL.[Mass] <> 0 
	OR CL.[Value] <> 0;

	
END
	
/*
BEGIN -- Updating document and deleting lines/entries
	INSERT INTO @D22([Id], [DocumentDate],	[Memo], [EvidenceTypeId])
	SELECT [Id], [DocumentDate],	[Memo], [EvidenceTypeId] 
	FROM dbo.Documents
	WHERE [DocumentTypeId] = N'purchasing-international' AND [SerialNumber] = 1;

	INSERT INTO @L22([Id], [DocumentId], [DocumentIndex], [LineTypeId], [ScalingFactor], [SortKey])
	SELECT DL.[Id], DL.[DocumentId], D22.[Index], DL.[LineTypeId], [ScalingFactor], [SortKey]
	FROM dbo.DocumentLines DL
	JOIN @D22 D22 ON D22.[Id] = DL.[DocumentId];

	INSERT INTO @E22([Id], [DocumentLineId], [DocumentIndex], [DocumentLineIndex], [EntryNumber], [Direction], [AccountId], [IfrsEntryClassificationId], [ResourceId], [Count], [MoneyAmount], [Value])
	SELECT DLE.[Id], L22.[Id], L22.DocumentIndex, L22.[Index], [EntryNumber], [Direction], [AccountId], [IfrsEntryClassificationId], [ResourceId], [Count], [MonetaryValue], [Value]
	FROM dbo.DocumentLineEntries DLE
	JOIN @L22 L22 ON L22.[Id] = DLE.[DocumentLineId];

	--SELECT * FROM @D22; SELECT * FROM @L22; SELECT * FROM @E22;

	UPDATE @E22 SET [Count] = [Count] / 2, [Value] = [Value] / 2 WHERE [Index] = 1;
	UPDATE @E22 SET [Count] = [Count] * 1.5, [Value] = [Value] * 1.5 + 1175000 WHERE [Index] = 2;
	UPDATE @L22 SET [ScalingFactor] = 3 WHERE [ScalingFactor] = 1;
	DELETE FROM @L22 WHERE [Index] = 1;
	DELETE FROM @L22 WHERE [Index] = 3;

	--SELECT * FROM @D22; SELECT * FROM @L22; SELECT * FROM @E22;

	EXEC [api].[Documents__Save]
		@DocumentTypeId = N'purchasing-international',
		@Documents = @D22, @Lines = @L22, @Entries = @E22,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Update and Delete'
		GOTO Err_Label;
	END;

	INSERT INTO @D22Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D22Ids) ORDER BY [SortKey], [EntryNumber];
END

BEGIN -- signing
	DECLARE @DocsToSign [dbo].[IndexedIdList]
	INSERT INTO @DocsToSign([Index], [Id]) SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents;-- WHERE STATE = N'Draft';

	EXEC [api].[Documents__Sign]
		@DocsIndexedIds = @DocsToSign, @ToState = N'Posted', @ReasonDetails = N'seems ok',
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	INSERT INTO @D23Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D23Ids) ORDER BY [SortKey], [EntryNumber];
	SELECT * FROM [rpt].[Documents__Signatures](@D23Ids);

	--select *, ValidFrom AT TIME ZONE 'UTC' AS [SavedAt]  from RoleMemberships;
	--select *, ValidFrom AT TIME ZONE 'UTC' AS [SavedAt] from RoleMembershipsHistory;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Sign'
		GOTO Err_Label;
	END;
END
*/