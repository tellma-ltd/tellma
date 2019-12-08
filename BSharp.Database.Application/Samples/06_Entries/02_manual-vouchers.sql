DECLARE @D [dbo].[DocumentList], @L [dbo].LineList, @E [dbo].EntryList, @DIds dbo.IdList;

--DECLARE @date1 date = '2017.02.01', @date2 date = '2022.02.01', @date3 datetime = '2023.02.01';
BEGIN -- Inserting
	INSERT INTO @D
	([Index],	[DocumentDate], [Memo],									[OperatingSegmentId]) VALUES
	(0,			'2018.01.01',	N'Capital investment',					@OS_IT),
	(1,			'2018.01.01',	N'Exchange of $50000',					@OS_IT),
	(2,			'2018.01.05',	N'Vehicles purchase receipt on account',@OS_IT),
	(3,			'2018.01.06',	N'Putting one vehicle into use',		@OS_IT),
	(4,			'2018.01.25',	N'Office Rental Q1',					@OS_IT);
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

	(4,			1,				N'ManualLine'),
	(5,			1,				N'ManualLine'),

	(6,			2,				N'ManualLine'),
	(7,			2,				N'ManualLine'),
	(8,			2,				N'ManualLine'),
	(9,			2,				N'ManualLine'),
	(10,		2,				N'ManualLine'),
	
	(11,		3,				N'ManualLine'),
	(12,		3,				N'ManualLine'),

	(13,		4,				N'ManualLine'),
	(14,		4,				N'ManualLine'),
	(15,		4,				N'ManualLine')
		;
	DECLARE @R_ETB INT = (SELECT [Id] FROM dbo.Resources WHERE CurrencyId = N'ETB' AND ResourceClassificationId = dbo.fn_RCCode__Id(N'Cash') AND DefinitionId = N'currencies')
	DECLARE @R_USD INT = (SELECT [Id] FROM dbo.Resources WHERE CurrencyId = N'USD' AND ResourceClassificationId = dbo.fn_RCCode__Id(N'Cash') AND DefinitionId = N'currencies')
	
	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [EntryNumber], [Direction],
				[AccountId],		[EntryClassificationId],	[ResourceId],[MonetaryValue],[Value]) VALUES
	(0, 0, 0,1,+1,@CBEUSD,			@ProceedsFromIssuingShares, 	@R_USD,		200000,			4700000),--
	(1, 1, 0,1,+1,@CBEUSD,			@ProceedsFromIssuingShares, 	@R_USD,		100,			2350),
	(2, 2, 0,1,-1,@CapitalMA,		@IssueOfEquity,					@R_ETB,		2351175,		2351175),
	(3, 3, 0,1,-1,@CapitalAA,		@IssueOfEquity,					@R_ETB,		2351175,		2351175),
		
	(4, 4, 1,1,+1,@CBEETB,			@InternalCashTransferExtension, @R_ETB,		1175000,		1175000),
	(5, 5, 1,1,-1,@CBEUSD,			@InternalCashTransferExtension,	@R_USD,		50000,			1175000);

	INSERT INTO @E ([Index], [LineIndex], [DocumentIndex], [EntryNumber], [Direction],
				[AccountId],	[EntryClassificationId],[ResourceId],[MonetaryValue], [ExternalReference], [AdditionalReference], [RelatedAgentId], [RelatedAmount]) VALUES
	(6, 6, 2,1,+1,@PPEWarehouse,@InventoryPurchaseExtension,@R_ETB,		600000,			N'C-14209',			NULL,					NULL,			NULL),--
	(7, 7, 2,1,+1,@VATInput,	NULL, 						@R_ETB,		90000,			N'C-14209',			N'FS010102',			@Toyota,		600000),--
	(8, 8, 2,1,+1,@PPEWarehouse,@InventoryPurchaseExtension,@R_ETB,		600000,			N'C-14209',			NULL,					NULL,			NULL),
	(9, 9, 2,1,+1,@VATInput,	NULL, 						@R_ETB,		90000,			N'C-14209',			N'FS010102',			@Toyota,		600000),
	(10,10,2,1,-1,@ToyotaAccount,NULL,						@R_ETB,		1380000,		N'C-14209',			NULL,					NULL,			NULL),

	(11,11,3,1,+1,@PPEVehicles,	@PPEAdditions,				@R_ETB,		600000,			NULL,				NULL, NULL, NULL),
	(12,12,3,1,-1,@PPEWarehouse,@InvReclassifiedAsPPE,		@R_ETB,		600000,			NULL,				NULL, NULL, NULL),
	
	(13,13,4,1,+1,@VATInput,	NULL,						@R_ETB,		2250,			N'C-25301',			N'BP188954',			@Regus,			15000),
	(14,14,4,1,+1,@PrepaidRental,NULL,						@R_ETB,		15000,			N'C-25301',			NULL,					NULL,			NULL),
	(15,15,4,1,-1,@RegusAccount,NULL, 						@R_ETB,		17250,			N'C-25301',			NULL,					NULL,			NULL);
	; 
	
	EXEC [api].[Documents__Save]
		@DefinitionId = N'manual-journals',
		@Documents = @D, @Lines = @L, @Entries = @E,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Insert'
		GOTO Err_Label;
	END;

	DECLARE @DocLinesIndexedIds dbo.[IndexedIdList];
	INSERT INTO @DocLinesIndexedIds([Index], [Id])
	-- TODO: fill index using ROWNUMBER
	SELECT [Id], [Id] FROM dbo.[Lines] WHERE [State] = N'Draft';

	--IF (1=0)
	EXEC [api].[Lines__Sign]
		@IndexedIds = @DocLinesIndexedIds,
		@ToState = N'Reviewed',
		@AgentId = @MohamadAkra,
		@RoleId = @Accountant, -- we allow selecting the role manually,
		@SignedAt = @Now,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Document Lines Signing'
		GOTO Err_Label;
	END;

	DECLARE @DocsIndexedIds dbo.[IndexedIdList];
	INSERT INTO @DocsIndexedIds([Index], [Id])
	-- TODO: fill index using ROWNUMBER
	SELECT [Id], [Id] FROM dbo.Documents WHERE [State] = N'Active';

	IF (1=0)
	EXEC [api].[Documents__File]
		@IndexedIds = @DocsIndexedIds,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @DebugManualVouchers = 1
	BEGIN
			INSERT INTO @DIds([Id]) SELECT [Id] FROM dbo.Documents;
			EXEC [rpt].[Docs__UI] @DIds;
	END
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
	LEFT JOIN dbo.AccountClassifications AC ON JS.AccountClassificationId = AC.Id
	ORDER BY AC.[Code], A.[Code]

	SELECT
		A.[Name] As [Supplier], 
		A.TaxIdentificationNumber As TIN, 
		J.ExternalReference As [Invoice #], J.AdditionalReference As [Cash M/C #],
		FORMAT(SUM(J.[Value]), '##,#.00;(##,#.00);-', 'en-us') AS VAT,
		FORMAT(SUM(J.[RelatedAmount]), '##,#.00;(##,#.00);-', 'en-us') AS [Taxable Amount],
		J.DocumentDate As [Invoice Date]
	FROM [dbo].[fi_Journal]('2018.01.02','2019.01.01') J
	LEFT JOIN [dbo].[Agents] A ON J.[RelatedAgentId] = A.Id
	WHERE J.[AccountId] = @VATInput
	GROUP BY A.[Name], A.TaxIdentificationNumber, J.ExternalReference, J.AdditionalReference, J.DocumentDate;
END


--SELECT * FROM dbo.DocumentSignatures;
--SELECT * FROM dbo.DocumentStatesHistory;
--IF (1=0)
--BEGIN -- Updating document and deleting lines/entries
--	INSERT INTO @D12([Index], [Id], [DocumentDate],	[Memo])
--	SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id], [DocumentDate],	[Memo]
--	FROM dbo.Documents
--	WHERE [DocumentDefinitionId] = N'manual-journals' AND [SerialNumber] = 1;

--	INSERT INTO @L12([Index], [DocumentIndex],					[Id], [DocumentId], [LineDefinitionId], [ScalingFactor], [SortKey])
--	SELECT ROW_NUMBER() OVER(ORDER BY DL.[Id]), D12.[Index], DL.[Id], DL.[DocumentId],  DL.[LineDefinitionId], [ScalingFactor], [SortKey]
--	FROM dbo.Lines DL
--	JOIN @D12 D12 ON D12.[Id] = DL.[DocumentId];

--	INSERT INTO @E12([Index], [Id], [LineId], [DocumentIndex], [LineIndex],				[EntryNumber], [Direction], [AccountId], [IfrsEntryClassificationId], [AgentId], [ResponsibilityCenterId], [ResourceId], [Quantity], [Count], [MonetaryValue], [Value])
--	SELECT ROW_NUMBER() OVER (ORDER BY DLE.[Id]), DLE.[Id], L12.[Id], L12.DocumentIndex, L12.[Index],	[EntryNumber], [Direction], [AccountId], [IfrsEntryClassificationId], [AgentId], [ResponsibilityCenterId], [ResourceId], [Quantity], [Count], [MonetaryValue], [Value]
--	FROM dbo.Entries DLE
--	JOIN @L12 L12 ON L12.[Id] = DLE.[LineId];

--	UPDATE @E12 SET [Count] = [Count] / 2, [Value] = [Value] / 2 WHERE [Index] = 1;
--	UPDATE @E12 SET [Count] = [Count] * 1.5, [Value] = [Value] * 1.5 + 1175000 WHERE [Index] = 2;
--	UPDATE @L12 SET [ScalingFactor] = 3 WHERE [ScalingFactor] = 1;
--	DELETE FROM @L12 WHERE [Index] = 1;
--	DELETE FROM @L12 WHERE [Index] = 3;

--	EXEC [api].[Documents__Save]
--		@DefinitionId = N'manual-journals',
--		@Documents = @D12, @Lines = @L12, @Entries = @E12,
--		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--	IF @ValidationErrorsJson IS NOT NULL 
--	BEGIN
--		Print 'Capital Investment (M): Update and Delete'
--		GOTO Err_Label;
--	END;

--	INSERT INTO @D12Ids([Id]) SELECT  [Id] FROM dbo.Documents;
--	SELECT * FROM rpt.Documents(@D12Ids) ORDER BY [SortKey], [EntryNumber];
--END

--BEGIN -- signing
--	DECLARE @DocsToSign [dbo].[IndexedIdList]
--	INSERT INTO @DocsToSign([Index], [Id]) SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents;-- WHERE STATE = N'Active';

--	EXEC [api].[Lines__Sign]
--		@DocsIndexedIds = @DocsToSign, @ToState = N'Reviewed', @ReasonDetails = N'seems ok',
--		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--	INSERT INTO @D13Ids([Id]) SELECT [Id] FROM dbo.Documents;
--	SELECT * FROM rpt.Documents(@D13Ids) ORDER BY [SortKey], [EntryNumber];
--	SELECT * FROM [rpt].[Documents__Signatures](@D13Ids);

--	IF @ValidationErrorsJson IS NOT NULL 
--	BEGIN
--		Print 'Capital Investment (M): Sign'
--		GOTO Err_Label;
--	END;
--END
