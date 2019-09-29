DECLARE @D41 [dbo].[DocumentList], @L41 [dbo].DocumentLineList, @E41 [dbo].DocumentLineEntryList;
DECLARE @D42 [dbo].[DocumentList], @L42 [dbo].DocumentLineList, @E42 [dbo].DocumentLineEntryList;
DECLARE @D41Ids dbo.[IdList], @D42Ids dbo.[IdList], @D43Ids dbo.[IdList];

BEGIN -- Inserting
	INSERT INTO @D41(
	[DocumentDate],	[Memo]) VALUES (
	'2017.01.05',		N'Sale of HSP products'
	);
	INSERT INTO @L41( --DocumentIndex DEFAULT 0
		[LineDefinitionId], [SortKey]) VALUES
		(N'GoodReceiptInTransitWithInvoice', 1),
		(N'GoodReceiptInTransitWithInvoice', 2),
		(N'CashIssue', 1);

	INSERT INTO @E41 (--DocumentIndex DEFAULT 0
[DocumentLineIndex], [Index], [EntryNumber], [Direction], [AccountId], [IfrsEntryClassificationId],			[ResourceId], [MonetaryValue],[Mass], [Value]) VALUES
		(0,				0, 1,				+1,			@ESL,	N'InventoryPurchaseExtension', 				@HR1000x1_9,	0,			500000, 0),
		(0,				1, 2,				-1,			@Vimeks,	NULL,									@USD,		100000,			0,		0),
		(1,				2, 1,				+1,			@ESL,	N'InventoryPurchaseExtension',				@CR1000x1_4,	0,			500000, 0),
		(1,				3, 2,				-1,			@Vimeks,	NULL,									@USD,		200000,			0,		0),
		(2,				4, 1,				+1,			@Vimeks,	NULL,									@USD,		300000,			0,		0),
		(2,				5, 2,				-1,			@CBELC,	N'PaymentsToSuppliersForGoodsAndServices',	@ETB,		9000000,		0,		0);

	EXEC [api].[Documents__Save]
		@DocumentTypeId = N'purchasing-international',
		@Documents = @D41, @Lines = @L41, @Entries = @E41,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Purchasing: Insert'
		GOTO Err_Label;
	END;

	INSERT INTO @D41Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D41Ids) ORDER BY [SortKey], [EntryNumber];

	WITH GritwiLines AS (
		SELECT
			SortKey, LineId, [LineDefinitionId], EntryNumber, Account, IfrsEntryClassificationId, [Resource], Direction, Quantity, [Unit], [Value]
		FROM rpt.Documents(@D41Ids)
		WHERE [LineDefinitionId] = N'GoodReceiptInTransitWithInvoice'
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
	-- TODO: add other unit types, and currency
	MUM.[Name] AS [MassUnit],
	CAST([MonetaryValue] AS MONEY) AS[MonetaryValue],
	CAST([Mass] AS MONEY) AS [Mass],
	CAST([Value] AS MONEY) AS [Value]
	FROM CompactLines CL
	JOIN dbo.Resources R ON CL.[ResourceId] = R.[Id]
	JOIN dbo.MeasurementUnits MUM ON R.MassUnitId = MUM.Id
	JOIN dbo.[AccountClassifications] A ON CL.[AccountId] = A.[Id]
	JOIN dbo.IfrsEntryClassifications IEC ON CL.IfrsEntryClassificationId = IEC.Id
	WHERE CL.[Quantity] <> 0 --OR CL.[MonetaryValue] <> 0 OR CL.[Mass] <> 0 
	OR CL.[Value] <> 0;

	
END
	
/*
BEGIN -- Updating document and deleting lines/entries
	INSERT INTO @D42([Id], [DocumentDate],	[Memo])
	SELECT [Id], [DocumentDate],	[Memo] 
	FROM dbo.Documents
	WHERE [DocumentTypeId] = N'purchasing-international' AND [SerialNumber] = 1;

	INSERT INTO @L42([Id], [DocumentId], [DocumentIndex], [LineDefinitionId], [ScalingFactor], [SortKey])
	SELECT DL.[Id], DL.[DocumentId], D42.[Index], DL.[LineDefinitionId], [ScalingFactor], [SortKey]
	FROM dbo.DocumentLines DL
	JOIN @D42 D42 ON D42.[Id] = DL.[DocumentId];

	INSERT INTO @E42([Id], [DocumentLineId], [DocumentIndex], [DocumentLineIndex], [EntryNumber], [Direction], [AccountId], [IfrsEntryClassificationId], [ResourceId], [Count], [MoneyAmount], [Value])
	SELECT DLE.[Id], L42.[Id], L42.DocumentIndex, L42.[Index], [EntryNumber], [Direction], [AccountId], [IfrsEntryClassificationId], [ResourceId], [Count], [MonetaryValue], [Value]
	FROM dbo.DocumentLineEntries DLE
	JOIN @L42 L42 ON L42.[Id] = DLE.[DocumentLineId];

	--SELECT * FROM @D42; SELECT * FROM @L42; SELECT * FROM @E42;

	UPDATE @E42 SET [Count] = [Count] / 2, [Value] = [Value] / 2 WHERE [Index] = 1;
	UPDATE @E42 SET [Count] = [Count] * 1.5, [Value] = [Value] * 1.5 + 1175000 WHERE [Index] = 2;
	UPDATE @L42 SET [ScalingFactor] = 3 WHERE [ScalingFactor] = 1;
	DELETE FROM @L42 WHERE [Index] = 1;
	DELETE FROM @L42 WHERE [Index] = 3;

	--SELECT * FROM @D42; SELECT * FROM @L42; SELECT * FROM @E42;

	EXEC [api].[Documents__Save]
		@DocumentTypeId = N'purchasing-international',
		@Documents = @D42, @Lines = @L42, @Entries = @E42,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Update and Delete'
		GOTO Err_Label;
	END;

	INSERT INTO @D42Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D42Ids) ORDER BY [SortKey], [EntryNumber];
END

BEGIN -- signing
	DECLARE @DocsToSign [dbo].[IndexedIdList]
	INSERT INTO @DocsToSign([Index], [Id]) SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents;-- WHERE STATE = N'Draft';

	EXEC [api].[Documents__Sign]
		@DocsIndexedIds = @DocsToSign, @ToState = N'Posted', @ReasonDetails = N'seems ok',
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	INSERT INTO @D43Ids([Id]) SELECT [Id] FROM dbo.Documents;
	SELECT * FROM rpt.Documents(@D43Ids) ORDER BY [SortKey], [EntryNumber];
	SELECT * FROM [rpt].[Documents__Signatures](@D43Ids);

	--select *, ValidFrom AT TIME ZONE 'UTC' AS [SavedAt]  from RoleMemberships;
	--select *, ValidFrom AT TIME ZONE 'UTC' AS [SavedAt] from RoleMembershipsHistory;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Sign'
		GOTO Err_Label;
	END;
END
*/