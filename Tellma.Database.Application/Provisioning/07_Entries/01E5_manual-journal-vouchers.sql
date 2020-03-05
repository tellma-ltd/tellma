--DECLARE @D5 [dbo].[DocumentList], @L5 [dbo].LineList, @E5 [dbo].EntryList, @DIds5 dbo.IdList;

--DECLARE @date1 date = '2017.02.01', @date2 date = '2022.02.01', @date3 datetime = '2023.02.01';
IF @DB = (N'105')  -- Simpex, SAR, en/ar
BEGIN -- Inserting
	INSERT INTO @D
	([Index],	[DocumentDate], [Memo]) VALUES
	(0,			'2019.12.31',	N'ترحيل أرصدة افتتاحية'),
	(1,			'2020.01.04',	N'فتح اعتماد 100 ألف دولار رقم 760 لصالح ستورا'),
	(2,			'2020.01.05',	N'استلام مستندات الشحن من ستورا'),
	(3,			'2020.01.06',	N'وصول جزئي اعتماد 760'),
	(4,			'2020.01.07',	N'تحويل من مخزن جدة للرياض'),
	(5,			'2020.01.08',	N'تسليم على الحساب لصالح مطبعة الرياض'),
	(6,			'2020.01.08',	N'بيع نقدي من مخزن جدة ');


	INSERT INTO @L
	([Index], [DocumentIndex], [DefinitionId]) VALUES
	(0,			0,				N'ManualLine'),
	(1,			0,				N'ManualLine'),
	(2,			0,				N'ManualLine'),
	(3,			0,				N'ManualLine');

	--(0,			1,				N'ManualLine'),
	--(1,			1,				N'ManualLine'),
	--(2,			1,				N'ManualLine')
	;
	/*
	INSERT INTO @L
	([Index], [DocumentIndex], [DefinitionId]) VALUES
	(0,			2,				N'ManualLine'),--
	(1,			2,				N'ManualLine'),--
	(2,			2,				N'ManualLine'),
	(3,			2,				N'ManualLine'),
	(4,			2,				N'ManualLine'),
	
	(0,			3,				N'ManualLine'),
	(1,			3,				N'ManualLine'),

	(0,			4,				N'ManualLine'),
	(1,			4,				N'ManualLine'),
	(2,			4,				N'ManualLine');
		;
		*/
	DECLARE @500Pkt INT = (SELECT [Id] FROM dbo.MeasurementUnits WHERE [Name] = N'500pkt');
	DECLARE @mt INT = (SELECT [Id] FROM dbo.MeasurementUnits WHERE [Name] = N'mt');

	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [Index], [Direction],
				[AccountId],	[EntryTypeId],							[AgentId],	[ResourceId],	[Quantity], [UnitId],	[Value]) VALUES
	(0, 0, 0,0,+1,@WH,			@InternalInventoryTransferExtension, 	@5WH_JED,	@R0,			1,			@mt,	10000),--
	(1, 1, 0,0,+1,@WH,			@InternalInventoryTransferExtension, 	@5WH_JED,	@R1,			10,			@500Pkt,		20000),
	(2, 2, 0,0,+1,@RJB_SAR,		@InternalCashTransferExtension,			NULL,		NULL,			NULL,		NULL,		30000),
	(3, 3, 0,0,-1,@5Capital,	@IssueOfEquity,							NULL,		NULL,			NULL,		NULL,		60000);
		
--	(4, 4, 1,1,+1,@BA_CBEETB,		@InternalCashTransferExtension, NULL,			1175000),
--	(5, 5, 1,1,-1,@SA_CBEUSD,		@InternalCashTransferExtension,	50000,			1175000);
/*
	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [Index], [Direction],
				[AccountId],	[EntryTypeId],				[Value], [ExternalReference], [AdditionalReference], [NotedAgentId], [NotedAmount]) VALUES
	(6, 6, 2,1,+1,@PPEWarehouse,@InventoryPurchaseExtension,600000,	N'C-14209',			NULL,					NULL,			NULL),		
	(7, 7, 2,1,+1,@VATInput,	NULL, 						90000,	N'C-14209',			N'FS010102',			@Toyota,		600000),
	(8, 8, 2,1,+1,@PPEWarehouse,@InventoryPurchaseExtension,600000,	N'C-14209',			NULL,					NULL,			NULL),	
	(9, 9, 2,1,+1,@VATInput,	NULL, 						90000,	N'C-14209',			N'FS010102',			@Toyota,		600000),
	(10,10,2,1,-1,@SA_ToyotaAccount,NULL,					1380000,	N'C-14209',			NULL,					NULL,			NULL),

	(11,11,3,1,+1,@PPEVehicles,	@PPEAdditions,				600000,	NULL,				NULL,					NULL,			NULL),
	(12,12,3,1,-1,@PPEWarehouse,@InvReclassifiedAsPPE,		600000,	NULL,				NULL,					NULL,			NULL),	
	
	(13,13,4,1,+1,@VATInput,	NULL,						2250,	N'C-25301',			N'BP188954',			@Regus,			15000),
	(14,14,4,1,+1,@PrepaidRental,NULL,						15000,	N'C-25301',			NULL,					NULL,			NULL),		
	(15,15,4,1,-1,@RegusAccount,NULL, 						17250,	N'C-25301',			NULL,					NULL,			NULL)
	; 
	*/
	EXEC [api].[Documents__Save]
		@DefinitionId = N'manual-journal-vouchers',
		@Documents = @D, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Simpex Documents: Insert: ' + @ValidationErrorsJson
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
		@ToState = 4, -- N'Ready To Post',
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