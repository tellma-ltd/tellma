IF @DB = N'101' -- Banan SD, USD, en
BEGIN -- Inserting
	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D
	([Index],	[PostingDate], [Memo]) VALUES
	(-1,		'2017.01.01',	N'B10 Subscription Agreements')
	;
--(0,52,	N'ContractId',		0,	N'Customer',		N'الزبون',				1,4,0),
--(1,52,	N'CenterId',		1,	N'Profit Center',	N'مركز الربح',			1,4,0),
--(2,52,	N'ResourceId',		1,	N'Service',			N'الخدمة',				1,4,0),
--(3,52,	N'Quantity',		1,	N'Duration',		N'الفترة',				1,4,1),
--(4,52,	N'UnitId',			1,	N'',				N'',					1,4,1),
--(5,52,	N'Time1',			1,	N'From',			N'ابتداء من',			1,4,1),
--(6,52,	N'Time2',			1,	N'Till',			N'حتى',					1,1,0),
--(7,52,	N'CurrencyId',		0,	N'Currency',		N'العملة',				1,4,0),
--(8,52,	N'MonetaryValue',	0,	N'Due Excl. VAT',	N'المطالبة بدون ق.م',	1,4,0),
--(9,52,	N'CenterId',		0,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1);
	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = -1, @DefinitionId = @LeaseOutPrepaidLD;
	UPDATE @WL
	SET
		[Memo] = N'Washm',
		[ContractId0] = @Washm,
		[CenterId1] = @C101_B10,
		[ResourceId1] = @MonthlySubscription,
		[Quantity1]	= 1,
		[UnitId1] = @Month,
		[CurrencyId0] = @USD,
		[MonetaryValue0] = 4985,
		[CenterId0] = @C101_INV
	WHERE [DocumentIndex] = -1 AND [Index] = 0;

	INSERT INTO @WL
	EXEC bll.LineDefinitionEntries__Pivot @index = 1, @DocumentIndex = -1, @DefinitionId = @LeaseOutPrepaidLD;
	UPDATE @WL
	SET
		[Memo]		= N'Taji',
		[ContractId0] = @Taji,
		[CenterId1] = @C101_B10,
		[ResourceId1] = @MonthlySubscription,
		[Quantity1]	= 1,
		[UnitId1] = @Month,
		[CurrencyId0] = @USD,
		[MonetaryValue0] = 3500,
		[CenterId0] = @C101_INV
	WHERE [DocumentIndex] = -1 AND [Index] = 1;

	INSERT INTO @L([Index], [DocumentIndex], [Id], 	[DefinitionId], [Memo])
	SELECT [Index], [DocumentIndex], [Id], 	[DefinitionId], [Memo]
	FROM @WL
	
	INSERT INTO @E
	EXEC [bll].[WideLines__Unpivot] @WL;

	UPDATE L
	SET
		L.PostingDate = IIF(D.[PostingDateIsCommon]=1, D.PostingDate,L.[PostingDate]),
		L.Memo = IIF(D.[MemoIsCommon]=1, COALESCE(D.Memo,L.[Memo]), L.[Memo])
	FROM @L L JOIN @D D ON L.[DocumentIndex] = D.[Index]

	EXEC [api].[Documents__Save]
		@DefinitionId = @lease_out_templatesDD,
		@Documents = @D, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Lease Out Template: Insert: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DECLARE @WashmRevenueTemplate INT = (
		SELECT L.[Id]
		FROM dbo.Lines L
		JOIN map.Documents () D ON L.[DocumentId] = D.[Id]
		WHERE D.[DocumentType] = 0
		AND L.[DefinitionId] = @LeaseOutPrepaidLD
		AND L.[Memo] =  N'Washm'
	)
--	GOTO Templates_DONE

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]) - 1, [Id] FROM dbo.Documents WHERE [State] = 0;
	-- Executing

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id]
	FROM dbo.Documents 
	WHERE DefinitionId = @lease_out_templatesDD
	AND [State] = 0;
		
	EXEC [api].[Documents__Close]
		@DefinitionId = @lease_out_templatesDD,
		@IndexedIds = @DocsIndexedIds,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Lease out templates closing: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
Templates_DONE:
END