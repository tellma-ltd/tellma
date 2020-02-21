IF @DB = N'101' -- Banan SD, USD, en
BEGIN -- Inserting
	INSERT INTO @D
	([Index],	[DocumentDate], [Memo]) VALUES
	(0,			'2019.01.01',	N'Meals'),
	(1,			'2019.01.01',	N'KSA ACCA Annual Fees'),
	(2,			'2019.01.02',	N'Received 10,000 USD from MA - Dec'),
	(3,			'2019.01.02',	N'Yahoo Business Mail Subscription Jan 2019'),
	(4,			'2019.01.03',	N'Maintenance'),
	(5,			'2019.01.03',	N'Meals'),
	(6,			'2019.01.05',	N'Sold USD'),
	(7,			'2019.01.05',	N'Charged phone for PR and Marketing'),
	(8,			'2019.01.05',	N'Garden maintenance'),
	(9,			'2019.01.05',	N'Electricity'),
	(10,		'2019.01.06',	N'Paid employees Income Tax for Feb-Dec 2018');

	INSERT INTO @L
	([Index], [DocumentIndex], [DefinitionId]) VALUES
	(0,			0,				N'ManualLine'),
	(1,			0,				N'ManualLine'),
	(0,			1,				N'ManualLine'),
	(1,			1,				N'ManualLine'),
	(0,			2,				N'ManualLine'),
	(1,			2,				N'ManualLine'),
	(0,			3,				N'ManualLine'),
	(1,			3,				N'ManualLine'),
	(0,			4,				N'ManualLine'),
	(1,			4,				N'ManualLine'),
	(0,			5,				N'ManualLine'),
	(1,			5,				N'ManualLine'),
	(0,			6,				N'ManualLine'),
	(1,			6,				N'ManualLine');

	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [EntryNumber], [Direction],
				[AccountId],	[EntryTypeId],										[AgentId],	[CurrencyId],	[MonetaryValue],	[Value]) VALUES
	(0, 0, 0,0,+1,@1Meals,		@AdministrativeExpense, 							@1Overhead,	@SDG,			665,				12.55),--
	(0, 1, 0,0,-1,@1GMFund,		@PaymentsToSuppliersForGoodsAndServices,			NULL,		@SDG,			665,				12.55),

	(0, 0, 1,0,+1,@1Education,	@AdministrativeExpense, 							@1Overhead,	@SAR,			513,				136.8),--
	(0, 1, 1,0,-1,@1KSAFund,	@PaymentsToSuppliersForGoodsAndServices,			NULL,		@SAR,			513,				136.8),

	(0, 0, 2,0,+1,@1GMFund,		@ProceedsFromBorrowingsClassifiedAsFinancingActivities,NULL,	@USD,			10000,				10000),--
	(0, 1, 2,0,-1,@1MAPayable,	NULL,												NULL,		NULL,			10000,				10000),

	(0, 0, 3,0,+1,@1DomainRegistration,	@AdministrativeExpense,						@1Overhead,	@USD,			NULL,				19.95),--
	(0, 1, 3,0,-1,@1MAPayable,	NULL,												NULL,		NULL,			NULL,				19.95),

	(0, 0, 4,0,+1,@1Maintenance,@AdministrativeExpense, 							@1Overhead,	@SDG,			500,				9.09),--
	(0, 1, 4,0,-1,@1GMFund,		@PaymentsToSuppliersForGoodsAndServices,			NULL,		@SDG,			500,				9.09),

	(0, 0, 5,0,+1,@1Meals,		@AdministrativeExpense, 							@1Overhead,	@SDG,			1380,				25.09),--
	(0, 1, 5,0,-1,@1GMFund,		@PaymentsToSuppliersForGoodsAndServices,			NULL,		@SDG,			1380,				25.09),

	(0, 0, 6,0,+1,@1GMFund,		@InternalCashTransfer, 								NULL,		@SDG,			111000,				2000),--
	(0, 1, 6,0,-1,@1GMFund,		@InternalCashTransfer,								NULL,		@USD,			2000,				2000);
	
	EXEC [api].[Documents__Save]
		@DefinitionId = N'manual-journal-vouchers',
		@Documents = @D, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Banan SD Documents: Insert: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

/*
	DECLARE @DocsIndexedIds dbo.[IndexedIdList];
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents WHERE [State] BETWEEN 0 AND 4;

	DECLARE @IdWithNewComment INT
	SELECT @IdWithNewComment = MIN([Id]) FROM dbo.Documents;

	EXEC api.[Document_Comment__Save]
		@DocumentId = @IdWithNewComment,
		@Comment = N'For your kind attention',
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	DECLARE @OnBehalfOfRoleId INT, @OnBehalfOfuserId INT;
	IF @DB = N'101' -- Banan SD, USD, en
	OR @DB = N'102' -- Banan ET, ETB, en
	BEGIN
		SELECT @OnBehalfOfRoleId = [Id] FROM dbo.Roles WHERE [Name] = N'Comptroller'
		SELECT @OnBehalfOfuserId= [Id] FROM dbo.Users WHERE [Email] = N'jiad.akra@banan-it.com'
	END
	IF @DB = N'103' -- Lifan Cars, ETB, en/zh
		SELECT @OnBehalfOfRoleId = [Id] FROM dbo.Roles WHERE [Name] = N'Administrator'
	IF @DB = N'104' -- Walia Steel, ETB, en/am
	BEGIN
		SELECT @OnBehalfOfRoleId = [Id] FROM dbo.Roles WHERE [Name] = N'Accountant'
		SELECT @OnBehalfOfuserId= [Id] FROM dbo.Users WHERE [Email] = N'sarabirhanuk@gmail.com'
	END
	EXEC [api].[Documents__Sign]
		@IndexedIds = @DocsIndexedIds,
		@ToState = 4, -- N'Reviewed',
		@OnBehalfOfuserId = @OnBehalfOfuserId,
		@RoleId = @OnBehalfOfRoleId, -- we allow selecting the role manually,
		@SignedAt = @Now,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Lines Signing: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DELETE FROM @DocsIndexedIds;
	INSERT INTO @DocsIndexedIds([Index], [Id])
	SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents WHERE [State] BETWEEN 0 AND 4;

	EXEC [api].[Documents__Close]
		@IndexedIds = @DocsIndexedIds,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Documents closing: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
	*/

END