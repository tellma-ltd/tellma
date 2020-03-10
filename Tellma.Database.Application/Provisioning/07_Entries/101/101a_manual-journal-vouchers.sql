IF @DB = N'101' -- Banan SD, USD, en
BEGIN -- Inserting
	DELETE FROM @D; DELETE FROM @L; DELETE FROM @E; DELETE FROM @WL;
	INSERT INTO @D
	([Index],	[DocumentDate], [Memo]) VALUES
	(2,			'2019.01.02',	N'Received 10,000 USD from MA - Dec'),
	(3,			'2019.01.02',	N'Yahoo Business Mail Subscription Jan 2019'),
	(6,			'2019.01.05',	N'Sold USD'),
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
	(0,			2,				N'ManualLine'),(1,			2,				N'ManualLine'),
	(0,			3,				N'ManualLine'),(1,			3,				N'ManualLine'),
	(0,			6,				N'ManualLine'),(1,			6,				N'ManualLine'),
	(0,			7,				N'ManualLine'),(1,			7,				N'ManualLine'),
	(0,			8,				N'ManualLine'),(1,			8,				N'ManualLine'),
	(0,			9,				N'ManualLine'),(1,			9,				N'ManualLine'),
	(0,			10,				N'ManualLine'),(1,			10,				N'ManualLine'),(2,			10,				N'ManualLine'),
	(0,			11,				N'ManualLine'),(1,			11,				N'ManualLine');

	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [Direction],
				[AccountId],	[EntryTypeId],										[AgentId],	[CurrencyId],	[MonetaryValue],	[Value]) VALUES

	(0, 0, 2,+1,@1GMFund,		@ProceedsFromBorrowingsClassifiedAsFinancingActivities,NULL,	@USD,			10000,				10000),--
	(0, 1, 2,-1,@1MAPayable,	NULL,												NULL,		NULL,			10000,				10000),

	(0, 0, 3,+1,@1DomainRegistration,	@AdministrativeExpense,						@1Overhead,	@USD,			19.95,				19.95),--
	(0, 1, 3,-1,@1MAPayable,	NULL,												NULL,		NULL,			19.95,				19.95),

	(0, 0, 6,+1,@1GMFund,		@InternalCashTransfer, 								NULL,		@SDG,			111000,				2000),--
	(0, 1, 6,-1,@1GMFund,		@InternalCashTransfer,								NULL,		@USD,			2000,				2000),

	(0, 0, 7,+1,@1Internet,	@AdministrativeExpense, 							@1Overhead,	@SDG,			250,				4.55),--
	(0, 1, 7,-1,@1GMFund,		@PaymentsToSuppliersForGoodsAndServices,			NULL,		@SDG,			250,				4.55),

	(0, 0, 8,+1,@1Maintenance,@AdministrativeExpense, 							@1Overhead,	@SDG,			50,					0.91),--
	(0, 1, 8,-1,@1GMFund,		@PaymentsToSuppliersForGoodsAndServices,			NULL,		@SDG,			50,					0.91),

	(0, 0, 9,+1,@1Utilities,	@AdministrativeExpense, 							@1Overhead,	@SDG,			2500,				45.45),--
	(0, 1, 9,-1,@1GMFund,		@PaymentsToSuppliersForGoodsAndServices,			NULL,		@SDG,			2500,				45.45),
	
	(0, 0, 10,+1,@1EITax,		NULL, 												NULL,		@SDG,			15843.78,			379.99),--
	(0, 1, 10,+1,@1EStax,		NULL,												NULL,		@SDG,			58,					1.23),
	(0, 2, 10,-1,@1BOK,		@PaymentsToAndOnBehalfOfEmployees,					NULL,		@SDG,			15901.4,			381.22),
	
	(0, 0, 11,+1,@1GMFund,	@ReceiptsFromSalesOfGoodsAndRenderingOfServices, 	NULL,		@USD,			2500,				2500),--
	(0, 1, 11,-1,@1AR,		NULL,												@It3am,		@USD,			2500,				2500);

	EXEC sys.sp_set_session_context 'UserId', @Jiad_akra;
	EXEC [api].[Documents__Save]
		@DefinitionId = N'manual-journal-vouchers',
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
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents WHERE [State] BETWEEN 0 AND 4;
		
	EXEC [api].[Documents__Sign]
		@IndexedIds = @DocsIndexedIds,
		@ToState = 4, -- N'Completed',
		@OnBehalfOfuserId = @Jiad_akra,
		@RuleType = N'ByRole',
		@RoleId = @1Comptroller, -- we allow selecting the role manually,
		@SignedAt = @Now,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Lines Signing: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
	
	EXEC [api].[Documents__Post]
		@IndexedIds = @DocsIndexedIds,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Documents posting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

END