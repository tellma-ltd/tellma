DECLARE @D [dbo].[DocumentList], @L [dbo].LineList, @E [dbo].EntryList, @DIds dbo.IdList;

--DECLARE @date1 date = '2017.02.01', @date2 date = '2022.02.01', @date3 datetime = '2023.02.01';
IF @DB IN (N'102', N'103', N'104')  -- ACME, USD, en/ar/zh
BEGIN -- Inserting
	INSERT INTO @D
	([Index],	[DocumentDate], [Memo]) VALUES
	(0,			'2018.01.01',	N'Capital investment'),
	(1,			'2018.01.01',	N'Exchange of $50000'),
	(2,			'2018.01.05',	N'Vehicles purchase receipt on account'),
	(3,			'2018.01.06',	N'Putting one vehicle into use'),
	(4,			'2018.01.25',	N'Office Rental Q1');
/*	
	(5,			'2018.01.27',	N'Vehicles Invoice payment'),
	(6,			'2018.01.30',	N'Rental payment'),
	(9,			'2018.02.01',	N'Vehicle 1 Reinforcement'),
	(13,		'2018.02.15',	N'Feb 2018 Overtime'),
	(14,		'2018.02.20',	N'Job 1 Hours Loging'),
	(15,		'2018.02.25',	N'Feb 2018 Paysheet'),
	(16,		'2018.02.28',	N'Feb 2018 Salaries Xfer'),
	(17,		'2018.02.28',	N'Feb 2018 Month Closing');

	INSERT INTO @D
	([Index],	[DocumentDate], [Frequency], [Repetitions],	[Memo]) VALUES
	(7,			@d1,			N'Monthly',		60,			N'Vehicles Depreciation'),
	(8,			'2017.02.01',	N'Monthly',		60,			N'Sales Point Rental'),
	(10,		@dU,			N'Monthly',		48,			N'Reverse Depreciation'),
	(11,		@dU,			N'Monthly',		60,			N'Vehicles Depreciation'),
	(12,		'2018.02.01',	N'Monthly',		60,			N'Employee Hire');
*/	

	INSERT INTO @L
	([Index], [DocumentIndex], [DefinitionId]) VALUES
	(0,			0,				N'ManualLine'),
	(1,			0,				N'ManualLine'),
	(2,			0,				N'ManualLine'),
	(3,			0,				N'ManualLine'),

	(0,			1,				N'ManualLine'),
	(1,			1,				N'ManualLine');

	INSERT INTO @L
	([Index], [DocumentIndex], [DefinitionId]) VALUES
	(6,			2,				N'ManualLine'),--
	(7,			2,				N'ManualLine'),--
	(8,			2,				N'ManualLine'),
	(9,			2,				N'ManualLine'),
	(10,		2,				N'ManualLine'),
	
	(11,		3,				N'ManualLine'),
	(12,		3,				N'ManualLine'),

	(13,		4,				N'ManualLine'),
	(14,		4,				N'ManualLine'),
	(15,		4,				N'ManualLine');
		;
	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [Index], [Direction],
				[AccountId],		[EntryTypeId],					[MonetaryValue],[Value]) VALUES
	(0, 0, 0,1,+1,@SA_CBEUSD,		@ProceedsFromIssuingShares, 	200000,			4700000),--
	(1, 1, 0,1,+1,@BA_CBEUSD,		@ProceedsFromIssuingShares, 	100,			2350),
	(2, 2, 0,1,-1,@CapitalMA,		@IssueOfEquity,					NULL,			2351175),
	(3, 3, 0,1,-1,@CapitalAA,		@IssueOfEquity,					NULL,			2351175),
		
	(4, 4, 1,1,+1,@BA_CBEETB,		@InternalCashTransferExtension, NULL,			1175000),
	(5, 5, 1,1,-1,@SA_CBEUSD,		@InternalCashTransferExtension,	50000,			1175000);

	-- In a manual JV, we assume the following columns for dumb accounts:
	-- Account, Debit, Credit, Memo
	-- For smart accounts, 
	---------------------- we will have dynamic properties as follows:
	-- If agentdefinition = N'tax-agencies' show: NotedAgent, NotedAmount, ExternalReference, and 
		-- if NotedAgentDefinition =  N'customers' show also ExternalReference, Invoice #
		-- if NotedAgentDefinition =  N'suppliers' show also AdditionalReference, Machine #
	-- If Contract type = N'OnHand',  show also NotedAgentName, Debit: To - Credi: From
	-- If Contract type = N'Payable', Credit, and AgentDefinition = N'suppliers', Credit, show External Reference: Invoice #
	-- If Contract type = N'Receivable', Credit, and AgentDefinition = N'customers', Debit, show External Reference: Invoice #
	-- Resource is always among the dynamic properties
	-- ResourceDefinition specifies where or not to show (Count, Mass, Volume, Time, DueDate)
	-- If ResourceClassificationEntryType is enforced, show Entry Classification
	-- If AgentDefinition is not null, Show Agent

	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [Index], [Direction],
				[AccountId],	[EntryTypeId],				[Value], [ExternalReference], [AdditionalReference], [NotedAgentId], [NotedAmount]) VALUES
	(6, 6, 2,1,+1,@PPEWarehouse,@InventoryPurchaseExtension,600000,	N'C-14209',			NULL,					NULL,			NULL),		
	(7, 7, 2,1,+1,@VATInput,	NULL, 						90000,	N'C-14209',			N'FS010102',			@Toyota,		600000),
	(8, 8, 2,1,+1,@PPEWarehouse,@InventoryPurchaseExtension,600000,	N'C-14209',			NULL,					NULL,			NULL),	
	(9, 9, 2,1,+1,@VATInput,	NULL, 						90000,	N'C-14209',			N'FS010102',			@Toyota,		600000),
	(10,10,2,1,-1,@SA_ToyotaAccount,NULL,					1380000,N'C-14209',			NULL,					NULL,			NULL),

	(11,11,3,1,+1,@PPEVehicles,	@PPEAdditions,				600000,	NULL,				NULL,					NULL,			NULL),
	(12,12,3,1,-1,@PPEWarehouse,@InvReclassifiedAsPPE,		600000,	NULL,				NULL,					NULL,			NULL),	
	
	(13,13,4,1,+1,@VATInput,	NULL,						2250,	N'C-25301',			N'BP188954',			@Regus,			15000),
	(14,14,4,1,+1,@PrepaidRental,NULL,						15000,	N'C-25301',			NULL,					NULL,			NULL),		
	(15,15,4,1,-1,@RegusAccount,NULL, 						17250,	N'C-25301',			NULL,					NULL,			NULL)
	; 
	
	EXEC [api].[Documents__Save]
		@DefinitionId = N'manual-journal-vouchers',
		@Documents = @D, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Insert: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	--DECLARE @DocLinesIndexedIds dbo.[IndexedIdList];
	--INSERT INTO @DocLinesIndexedIds([Index], [Id])
	---- TODO: fill index using ROWNUMBER
	--SELECT [Id], [Id] FROM dbo.[Lines] WHERE [State] = 0; --N'Draft';

	--EXEC [api].[Lines__Sign]
	--	@IndexedIds = @DocLinesIndexedIds,
	--	@ToState = 4, -- N'Ready To Post',
	--	@AgentId = @MohamadAkra,
	--	@RoleId = @Accountant, -- we allow selecting the role manually,
	--	@SignedAt = @Now,
	--	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

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

	EXEC [api].[Documents__Post]
		@IndexedIds = @DocsIndexedIds,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Documents closing: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	IF @DebugManualVouchers = 1
	BEGIN
			DELETE FROM @DIds;
			INSERT INTO @DIds([Id]) SELECT [Id] FROM dbo.Documents WHERE DefinitionId = N'manual-journal-vouchers';
			EXEC [rpt].[Docs__UI] @DIds;

			SELECT * FROM map.DocumentSignatures();
			SELECT * FROM dbo.DocumentAssignmentsHistory;
	END
	IF @DebugReports = 1
	BEGIN
		SELECT AC.[Code], AC.[Name] AS [Classification],
			A.[Name] AS [Account],
			Format(Opening, '##,#.00;(##,#.00);-', 'en-us') AS Opening,
			Format(Debit, '##,#.00;-;-', 'en-us') AS Debit,
			Format(Credit, '##,#.00;-;-', 'en-us') AS Credit,
			Format(Closing , '##,#.00;(##,#.00);-', 'en-us') AS Closing
		FROM [rpt].[Accounts__TrialBalance] ('2018.01.02','2019.01.01') JS
		JOIN dbo.Accounts A ON JS.AccountId = A.Id
		LEFT JOIN dbo.[LegacyClassifications] AC ON JS.[LegacyClassificationId] = AC.Id
		ORDER BY AC.[Code], A.[Code]

		SELECT
			A.[Name] As [Supplier], 
			A.TaxIdentificationNumber As TIN, 
			J.ExternalReference As [Invoice #], J.AdditionalReference As [Cash M/C #],
			FORMAT(SUM(J.[Value]), '##,#.00;(##,#.00);-', 'en-us') AS VAT,
			FORMAT(SUM(J.[NotedAmount]), '##,#.00;(##,#.00);-', 'en-us') AS [Taxable Amount],
			J.DocumentDate As [Invoice Date]
		FROM [rpt].[Entries]('2018.01.02', '2019.01.01') J

		LEFT JOIN [dbo].[Agents] A ON J.[NotedAgentId] = A.Id
		WHERE J.[AccountId] = @VATInput
		GROUP BY A.[Name], A.TaxIdentificationNumber, J.ExternalReference, J.AdditionalReference, J.DocumentDate;
	END
END