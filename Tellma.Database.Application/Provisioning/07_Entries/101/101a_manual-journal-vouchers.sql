IF @DB = N'101' -- Banan SD, USD, en
BEGIN -- Inserting
	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D
	([Index],	[PostingDate], [Memo]) VALUES
	(2,			'2019.01.02',	N'Received 10,000 USD from MA - Dec'),
	(3,			'2019.01.02',	N'Yahoo Business Mail Subscription Jan 2019'),
	(7,			'2019.01.05',	N'Charged phone for PR and Marketing'),
	(8,			'2019.01.05',	N'Garden maintenance'),
	(9,			'2019.01.05',	N'Electricity'),
	(10,		'2019.01.06',	N'Paid employees Income Tax for Feb-Dec 2018'),
	(11,		'2019.01.06',	N'Received remaining amount of second Itaam Invoice')

	--(13,		'2019.01.06',	N'Sold USD and received in BOK'),
	--(13,		'2019.01.06',	N'Employees Dec 2018 Salaries Payment'),
	--(13,		'2019.01.06',	N'Received 58,451 AED from Mahara for period Jan 1, 2019 - Jun 30, 2019')
	--(13,		'2019.01.06',	N'Sold USD and received in BOK'),
	--(13,		'2019.01.06',	N'Sold USD and received in BOK'),
	--(13,		'2019.01.06',	N'Sold USD and received in BOK'),
	--(13,		'2019.01.06',	N'Sold USD and received in BOK'),
	--(13,		'2019.01.06',	N'Sold USD and received in BOK'),
	--(13,		'2019.01.06',	N'Sold USD and received in BOK')
	;

	INSERT INTO @L
	([Index], [DocumentIndex], [DefinitionId]) VALUES
	(0,			2,				@ManualLineDef),
	(0,			3,				@ManualLineDef),
	(0,			7,				@ManualLineDef),
	(0,			8,				@ManualLineDef),
	(0,			9,				@ManualLineDef),
	(0,			10,				@ManualLineDef),
	(0,			11,				@ManualLineDef);

	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [Direction],
				[AccountId],	[EntryTypeId],										[CenterId],	[CurrencyId],	[MonetaryValue],	[Value], [ContractId]) VALUES

	(0, 0, 2,+1,@1GMFund,		@ProceedsFromBorrowingsClassifiedAsFinancingActivities,@C101_INV,@USD,			10000,				10000,		NULL),
	(1, 0, 2,-1,@1MAPayable,	NULL,												@C101_INV,	NULL,			10000,				10000,		NULL),

	(0, 0, 3,+1,@1DomainRegistration,NULL,											NULL,		@USD,			19.95,				19.95,		NULL),
	(1, 0, 3,-1,@1MAPayable,	NULL,												@C101_INV,	NULL,			19.95,				19.95,		NULL),

	(0, 0, 7,+1,@1Internet,		NULL,			 									@C101_Sys,	@SDG,			250,				4.55,		NULL),
	(1, 0, 7,-1,@1GMFund,		@PaymentsToSuppliersForGoodsAndServices,			@C101_INV,	@SDG,			250,				4.55,		NULL),

	(0, 0, 8,+1,@1Maintenance,	@AdministrativeExpense, 							@C101_EXEC,	@SDG,			50,					0.91,		NULL),
	(1, 0, 8,-1,@1GMFund,		@PaymentsToSuppliersForGoodsAndServices,			@C101_INV,	@SDG,			50,					0.91,		NULL),

	(0, 0, 9,+1,@1Electricity,	NULL, 												NULL,		NULL,			2500,				45.45,		NULL),
	(1, 0, 9,-1,@1GMFund,		@PaymentsToSuppliersForGoodsAndServices,			@C101_INV,	@SDG,			2500,				45.45,		NULL),
	
	(0, 0, 10,+1,@1EITax,		NULL, 												@C101_INV,	@SDG,			15843.78,			379.99,		NULL),
	(1, 0, 10,+1,@1EStax,		NULL,												@C101_INV,	@SDG,			58,					1.23,		NULL),
	(2, 0, 10,-1,@1BOK,			@PaymentsToAndOnBehalfOfEmployees,					@C101_INV,	@SDG,			15901.4,			381.22,		NULL),
	
	(0, 0, 11,+1,@1GMFund,		@ReceiptsFromSalesOfGoodsAndRenderingOfServices, 	@C101_INV,	@USD,			2500,				2500,		NULL),
	(1, 0, 11,-1,@1AR,			NULL,												@C101_INV,	@USD,			2500,				2500,		@It3am);

	UPDATE L
	SET
		L.PostingDate = IIF(D.[PostingDateIsCommon]=1, D.PostingDate,L.[PostingDate]),
		L.Memo = IIF(D.[MemoIsCommon]=1, COALESCE(D.Memo,L.[Memo]), L.[Memo])
	FROM @L L JOIN @D D ON L.[DocumentIndex] = D.[Index]

	EXEC sys.sp_set_session_context 'UserId', @Jiad_akra;
	EXEC [api].[Documents__Save]
		@DefinitionId = @manual_journal_vouchersDef,
		@Documents = @D,
		@Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Banan SD Documents: Insert: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id]
	FROM dbo.Documents 
	WHERE DefinitionId = @manual_journal_vouchersDef
	AND [State] = 0;
		
	EXEC [api].[Documents__Close]
		@DefinitionId = @manual_journal_vouchersDef,
		@IndexedIds = @DocsIndexedIds,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Manual JVs closing: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

END