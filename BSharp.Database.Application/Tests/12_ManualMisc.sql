DECLARE @DM1 [dbo].[DocumentList], @LM1 [dbo].DocumentLineList, @EM1 [dbo].DocumentLineEntryList;
DECLARE @DM2 [dbo].[DocumentList], @LM2 [dbo].DocumentLineList, @EM2 [dbo].DocumentLineEntryList;
DECLARE @DM1Ids dbo.[IdList], @DM2Ids dbo.[IdList], @DM3Ids dbo.[IdList];

BEGIN -- Inserting
 -- N'Exchange of $50000'
	INSERT INTO @DM1([Index], [SortKey],
	[Memo],					[DocumentDate]) VALUES
(0,2,N'Exchange of $50000',	'2017.01.01');
INSERT INTO @LM1 ([Index], [DocumentIndex],	
			[LineTypeId],	[SortKey]) VALUES
	(0,	0, N'ManualLine',	1), 
	(1,	0, N'ManualLine',	2);
INSERT INTO @EM1 ([Index], [DocumentLineIndex], [DocumentIndex], [EntryNumber], [Direction],
				[AccountId], [IfrsEntryClassificationId],		[Value], [MonetaryValue]) VALUES
	(0,0,0,1,+1,@CBEETB,	N'InternalCashTransferExtension', 	1175000, 0),
	(1,1,0,1,-1,@CBEUSD,	N'InternalCashTransferExtension',	1175000, 50000); 
-- N'Vehicles purchase receipt on account w/invoice'
	INSERT INTO @DM1([Index], [SortKey],
	[Memo],							[DocumentDate]) VALUES
(1,3,N'Vehicles purchase receipt on account','2017.01.05');
INSERT INTO @LM1 ([Index], [DocumentIndex],	
			[LineTypeId],	[SortKey]) VALUES
	(2,	1, N'VATInvoiceWithGoodReceipt',	1), 
	(3,	1, N'VATInvoiceWithGoodReceipt',	2),
	(4,	1, N'ManualLine',	3)
	;
	-- TODO: Add the invoice number in ExternalReference
DECLARE @Camry2018_101 INT = (SELECT [Id] FROM dbo.ResourcePicks WHERE [ResourceId] = @Camry2018 AND [Code] = N'101')
DECLARE @Camry2018_102 INT = (SELECT [Id] FROM dbo.ResourcePicks WHERE [ResourceId] = @Camry2018 AND [Code] = N'102')
INSERT INTO @EM1 ([Index], [DocumentLineIndex], [DocumentIndex], [EntryNumber], [Direction],
				[AccountId],	[IfrsEntryClassificationId],	[ResourceId],	[ResourcePickId], [Value], [ExternalReference]) VALUES
	(2,2,1,1,+1,@PPEWarehouse,	N'InventoryPurchaseExtension', 	@Camry2018,		@Camry2018_101,		600000, N'C-14209'),
	(3,2,1,2,+1,@VATInput,		NULL, 							NULL,			NULL,				90000, N'C-14209'),
	(4,3,1,1,+1,@PPEWarehouse,	N'InventoryPurchaseExtension', 	@Camry2018,		@Camry2018_102,		600000, N'C-14209'),
	(5,3,1,2,+1,@VATInput,		NULL, 							NULL,			NULL,				90000, N'C-14209'),
	(6,4,1,1,-1,@ToyotaAccount,	NULL,							NULL,			NULL,				1380000, NULL);

-- Converting the inventory into
	INSERT INTO @DM1([Index], [SortKey],
	[Memo],							[DocumentDate]) VALUES
(2,4,N'Putting one vehicle into use','2017.01.06');

INSERT INTO @LM1 ([Index], [DocumentIndex],	
			[LineTypeId],						[SortKey]) VALUES
	(7,	2, N'ManualLine',	1), 
	(8,	2, N'ManualLine',	2);

DECLARE @Car1PPE INT = (SELECT [Id] FROM dbo.ResourcePicks WHERE [ResourceId] = @CarsRC AND [Code] = N'101');
INSERT INTO @EM1 ([Index], [DocumentLineIndex], [DocumentIndex], [EntryNumber], [Direction],
				[AccountId],	[IfrsEntryClassificationId],										[ResponsibilityCenterId], [ResourceId],	[ResourcePickId],	[Value], [Length]) VALUES
	(7,7,2,1,+1,@PPEVehicles,	'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment', @ExecutiveOfficeOps,	@CarsRC,	@Car1PPE,			600000,		200000),
	(8,8,2,1,-1,@PPEWarehouse,	N'InventoryToFixedAssetExtension',										NULL,					@Camry2018,	@Camry2018_101,		600000,		0);

-- N'Office rental invoice'
	INSERT INTO @DM1([Index], [SortKey],
	[Memo],				[DocumentDate]) VALUES
(3,5,N'Office Rental Q1','2017.01.25');
INSERT INTO @LM1 ([Index], [DocumentIndex],	
			[LineTypeId],				[SortKey]) VALUES
	(9,	3, N'VATInvoiceWithoutGoodReceipt',	1),
	(10,3, N'ManualLine',	2);
INSERT INTO @EM1 ([Index], [DocumentLineIndex], [DocumentIndex], [EntryNumber], [Direction],
				[AccountId],	[IfrsEntryClassificationId],			[Value], [ExternalReference]) VALUES
	(9,9,3,1,+1,@VATInput,			NULL,								2250, N'C-25301'),
	(10,9,3,2,+1,@PrepaidRental,		NULL,							15000, N'C-25301'),
	(11,9,3,3,-1,@RegusAccount,	NULL, 									17250, N'C-25301');
--	(12,10,3,2,-1,@CBEETB,	N'PaymentsToSuppliersForGoodsAndServices',	17250, NULL);

-- N'Salaries Accruals'
	INSERT INTO @DM1([Index], [SortKey],
	[Memo],				[DocumentDate]) VALUES
(4,6,N'Payroll Calculation','2018.02.01');
INSERT INTO @LM1 ([Index], [DocumentIndex],	
			[LineTypeId],				[SortKey]) VALUES
	(11,4, N'ManualLine',	1),
	(12,4, N'ManualLine',	2);
INSERT INTO @EM1 ([Index], [DocumentLineIndex], [DocumentIndex], [EntryNumber], [Direction],
				[AccountId],		[IfrsEntryClassificationId],	[Value],[ResourceId], [Time], [ResponsibilityCenterId], [AgentId]) VALUES
	(12,11,4,1,+1,@SalariesAdmin,			N'WagesAndSalaries',	7000,	NULL,			 0,		@SalesOpsAG,			NULL),
	(13,11,4,2,-1,@SalariesAccrualsTaxable,	NULL,					1500,	@Transportation, 0,		NULL,					@Mestawet),
	(14,11,4,3,-1,@SalariesAccrualsTaxable,	NULL,					5000,	@Basic,			0,		NULL,					@Mestawet),
	(15,11,4,3,-1,@SalariesAccrualsNonTaxable,NULL,					500,	@Transportation, 0,		NULL,					@Mestawet),
	(16,12,4,1,+1,@OvertimeAdmin,			N'WagesAndSalaries',	1000,	NULL,			0,		@SalesOpsAG,			NULL),
	(17,12,4,2,-1,@SalariesAccrualsTaxable,	NULL,					1000,	@HOvertime,		10,		NULL,					@Mestawet);

	EXEC [api].[Documents__Save]
		@DefinitionId = N'manual-journals',
		@Documents = @DM1, @Lines = @LM1, @Entries = @EM1,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Misc Journals (M): Insert'
		GOTO Err_Label;
	END;

-- N'Payroll Calculation'
	INSERT INTO @DM2([Index], [SortKey],
	[Memo],				[DocumentDate]) VALUES
(0,7,N'Paysheet Jan 2019','2017.02.01');

	WITH EmployeesAccruals AS (
		SELECT ROW_NUMBER() OVER (ORDER BY [AgentId], [ResourceId], [AccountId]) AS [Index],
			ROW_NUMBER() OVER (PARTITION BY [AgentId] ORDER BY [ResourceId], [AccountId]) AS [EntryNumber],
		[AccountId], -SUM([Direction] * [Value]) AS ValueBalance, SUM([Direction] * [Time]) AS TimeBalance, [ResourceId], [AgentId]
		FROM dbo.DocumentLineEntries
		WHERE [AccountId] IN (@SalariesAccrualsTaxable, @SalariesAccrualsNonTaxable)
		GROUP BY [AccountId], [ResourceId], [AgentId]
		HAVING SUM([Value]) <> 0
	),
	LineIndices AS (
		SELECT ROW_NUMBER() OVER (ORDER BY [AgentId]) AS [DocumentLineIndex], [AgentId]
		FROM EmployeesAccruals
		GROUP BY [AgentId]
	),
	EmployeesTaxableIncomes AS (
		SELECT [AgentId], SUM([ValueBalance]) AS TaxableAmount
		FROM EmployeesAccruals
		WHERE [AccountId] = @SalariesAccrualsTaxable
		GROUP BY [AgentId]
	),
	EmployeeIncomeTaxes AS (
		SELECT [AgentId], [bll].[fn_EmployeeIncomeTax]([AgentId], [TaxableAmount]) AS [EmployeeIncomeTax]
		FROM EmployeesTaxableIncomes
	)
	INSERT INTO @EM2([Index], [DocumentLineIndex], [EntryNumber], [Direction],[AccountId], [Value], [ResourceId], [AgentId], [Time])
	SELECT [Index], [DocumentLineIndex], [EntryNumber],
		CAST(SIGN([ValueBalance]) AS SMALLINT) AS [Direction], [AccountId], CAST(ABS([ValueBalance]) AS MONEY) AS [ValueBalance], [ResourceId], E.[AgentId], CAST([TimeBalance] AS MONEY) AS [TimeBalance]
	FROM EmployeesAccruals E 
	JOIN LineIndices L ON E.AgentId = L.AgentId
	UNION
	SELECT EA.[Index], L.[DocumentLineIndex], EA.[EntryNumber], -1 AS [Direction], @EmployeesIncomeTaxPayable, [EmployeeIncomeTax], NULL, NULL, 0
	FROM EmployeeIncomeTaxes EIT 
	JOIN LineIndices L ON EIT.AgentId = L.AgentId
	JOIN (
		SELECT [AgentId], (MAX([Index]) + 1) AS [Index], (MAX([EntryNumber]) + 1) AS [EntryNumber]
		FROM EmployeesAccruals
		GROUP BY [AgentId]
	) EA ON EIT.AgentId = EA.[AgentId]
	   
	INSERT INTO @LM2 ([Index], [DocumentIndex],	[LineTypeId],[SortKey])
	SELECT	[DocumentLineIndex], 0 AS [DocumentIndex], N'ManualLine', [DocumentLineIndex] 
	FROM @EM2
	GROUP BY [DocumentLineIndex]
	;
	--SELECT * FROM @DM2; SELECT * FROM @LM2; SELECT * FROM @EM2;
	EXEC [api].[Documents__Save]
		@DefinitionId = N'manual-journals',
		@Documents = @DM2, @Lines = @LM2, @Entries = @EM2,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Misc Journals (M): Insert'
		GOTO Err_Label;
	END;
END	
/*
(3,4,N'Vehicles Invoice payment',	N'OneTime',		0,	'2017.01.15'),

(5,6,N'Rental payment',				N'OneTime',		0,	'2017.01.30'),
(7,8,N'Vehicles Depreciation',		N'Monthly',		60,	@d1),
(8,9,N'Sales Point Rental',			N'Monthly',		60,	'2017.02.01'),
(9,10,N'Vehicle 1 Reinforcement',	N'OneTime',		0,	@dU),
(10,11,N'Reverse Depreciation',		N'Monthly',		48,	@dU),
(11,12,N'Vehicles Depreciation',	N'Monthly',		60,	@dU),
(12,13,N'Employee Hire',			N'Monthly',		60,	'2018.02.01'),
(13,14,N'Feb 2018 Overtime',		N'OneTime',		0,	'2018.02.15'),
(14,15,N'Job 1 Hours Loging',		N'OneTime',		0,	'2018.02.20'),
(15,16,N'Feb 2018 Paysheet',		N'OneTime',		0,	'2018.02.25'),
(16,17,N'Feb 2018 Salaries Xfer',	N'OneTime',		0,	'2018.02.28'),
(17,18,N'Feb 2018 Month Closing',	N'OneTime',		0,	'2018.02.28');

	--(2), (3), (4), (5), (6), (7), (8), (9), (10), (11), (12),
	--(13), 
	--(14), (15),
	--(16), (16), (16), (16), (16), (16),
	--(17);--, (18), (19), (20);

SELECT @Frequency = N'Monthly';
SELECT
	@P1_2 = 
		CASE 
			WHEN @Frequency = N'Daily' THEN DATEDIFF(DAY, @d1, @d2)
			WHEN @Frequency = N'Monthly' THEN DATEDIFF(MONTH, @d1, @d2)
		END,
	@P1_U = 
		CASE 
			WHEN @Frequency = N'Daily' THEN DATEDIFF(DAY, @d1, @dU)
			WHEN @Frequency = N'Monthly' THEN DATEDIFF(MONTH, @d1, @dU)
		END,		
	@P2_3 = 
		CASE 
			WHEN @Frequency = N'Daily' THEN DATEDIFF(DAY, @d2, @d3)
			WHEN @Frequency = N'Monthly' THEN DATEDIFF(MONTH, @d2, @d3)
		END,
	@PU_3 = 
		CASE 
			WHEN @Frequency = N'Daily' THEN DATEDIFF(DAY, @dU, @d3)
			WHEN @Frequency = N'Monthly' THEN DATEDIFF(MONTH, @dU, @d3)
		END;
SELECT @VR1_2 = CAST(300000 AS DECIMAL(38,22))/@P1_2;	
SELECT @VRU_3 = (420000 - @P1_U * @VR1_2)/@PU_3;

INSERT INTO @ESave -- Purchases and Rentals
([LineIndex],EntryNumber,OperationId,AccountId,		AgentId,		ResourceId,	Direction, Amount,	[Value],	NoteId,							[Reference],	[RelatedReference], [RelatedAgentId], [RelatedAmount]) VALUES
-- Invoice for vehicles
(	3,	1,	@WSI,	N'GoodsAndServicesReceivedFromSupplierButNotBilled',@Lifan,@Camry2018,+1,2,	600000,		NULL,							NULL,			NULL,				NULL,					NULL),
(	3,	2,	@WSI,	N'CurrentValueAddedTaxReceivables',@ERCA,	@ETB,			+1,		90000,	NULL,		NULL,							N'INV-YM01',	N'FS0987',			@Lifan,					600000),
(	3,	3,	@WSI,	N'CurrentPayablesForPurchaseOfNoncurrentAssets',@Lifan,@ETB,-1,		690000,	NULL,		NULL,							NULL,			NULL,				NULL,					NULL),
-- Vehicles Invoice payment
(	4,	1,	@WSI,	N'CurrentPayablesForPurchaseOfNoncurrentAssets',@Lifan,@ETB,+1,		690000,	NULL,		NULL,							NULL,			NULL,				NULL,					NULL),
(	4,	2,	@WSI,	N'CurrentWithholdingTaxPayable',@ERCA,		@ETB,			-1,		12000,	NULL,		NULL,							N'WT0901',		NULL,				@Lifan,					600000),
(	4,	3,	@WSI,	N'BalancesWithBanks',		@CBEETB,		@ETB,			-1,		678000,	NULL,		N'PurchaseOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities',N'Ck002',NULL,NULL,		NULL),
-- Invoice for rental
(	5,	1,	@Sales,		N'RentAccrualClassifiedAsCurrent',@Regus,	@Goff,			+1,		6,		24000,		NULL,							NULL,			NULL,				NULL,					NULL),
(	5,	2,	@Sales,		N'CurrentValueAddedTaxReceivables',@ERCA,	@ETB,			+1,		3600,	NULL,		NULL,							N'INV-YM02',	N'FS10117',			@Regus,					12000),
(	5,	3,	@Sales,		N'CurrentPayablesToLessors',@Regus,			@ETB,			-1,		27600,	NULL,		NULL,							NULL,			NULL,				NULL,					NULL),
-- Rental payment
(	6,	1,	@Sales,		N'CurrentPayablesToLessors',@Regus,			@ETB,			+1,		27600,	NULL,		NULL,							NULL,			NULL,				NULL,					NULL),
(	6,	2,	@Sales,		N'CurrentWithholdingTaxPayable',@ERCA,		@ETB,			-1,		480,	NULL,		NULL,							N'WT0902',		NULL,				@Regus,					12000),
(	6,	3,	@Sales,		N'BalancesWithBanks',		@CBEETB,		@ETB,			-1,		27120,	NULL,		N'PaymentsToSuppliersForGoodsAndServices',N'Ck003',	NULL,			NULL,					NULL),
-- Vehicles Depreciation
(	8,	1,	@ExecOffice,N'AdministrativeExpense',	@ExecutiveOffice,@Car1Svc,		+1,		+1,		@VR1_2,		N'DepreciationExpense',			NULL,			NULL,				NULL,					NULL),
(	8,	2,	@ExecOffice,N'MotorVehicles',			@ExecutiveOffice,@Car1Svc,		-1,		+1,		@VR1_2,		N'DepreciationPropertyPlantAndEquipment',NULL,	NULL,				NULL,					NULL),
-- Sales Point Rental
(	9,	1,	@Sales,		N'DistributionCosts',		@SalesAndMarketing,	@Goff,			+1,		+1,		4000,		N'OfficeSpaceExpense',			NULL,			NULL,				NULL,					NULL),
(	9,	2,	@Sales,		N'RentAccrualClassifiedAsCurrent',@Regus,	@Goff,			-1,		+1,		4000,		NULL,							NULL,			NULL,				NULL,					NULL),
-- Vehicle 1 Reinforcement
(	10,	1,	@ExecOffice,N'MotorVehicles',			@ExecutiveOffice,@Car1Svc,		+1,		@P2_3,	120000,		N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment',NULL,NULL,NULL,	NULL),
(	10,	2,	@WSI,		N'BalancesWithBanks',		@CBEETB,		@ETB,			-1,		120000,	NULL,		N'PurchaseOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities',N'Ck001',NULL,NULL,	NULL),
-- Reverse Depreciation
(	11,	1,	@ExecOffice,N'AdministrativeExpense',	@ExecutiveOffice,@Car1Svc,		+1,		-1,		-@VR1_2,	N'DepreciationExpense',			NULL,			NULL,				NULL,					NULL),
(	11,	2,	@ExecOffice,N'MotorVehicles',			@ExecutiveOffice,@Car1Svc,		-1,		-1,		-@VR1_2,	N'DepreciationPropertyPlantAndEquipment',NULL,	NULL,				NULL,					NULL),
---- Vehicles Depreciation
(	12,	1,	@ExecOffice,N'AdministrativeExpense',	@ExecutiveOffice,@Car1Svc,		+1,		+1,		@VRU_3,		N'DepreciationExpense',			NULL,			NULL,				NULL,					NULL),
(	12,	2,	@ExecOffice,N'MotorVehicles',			@ExecutiveOffice,@Car1Svc,		-1,		+1,		@VRU_3,		N'DepreciationPropertyPlantAndEquipment',NULL,	NULL,				NULL,					NULL);
INSERT INTO @ESave -- HR, Employment Contract
([LineIndex],EntryNumber,OperationId,AccountId,		AgentId,		ResourceId,	Direction, Amount,	[Value],	NoteId,							[Reference],	[RelatedReference], [RelatedAgentId], [RelatedAmount]) VALUES
-- Employee Hire: similar to depreciation and rental.
(	13,	1,	@Production,N'OtherInventories',		@Production,@Labor,		+1,		+208,	18870,		NULL,							NULL,			NULL,				@MesfinWolde,			NULL),
(	13,	2,	@Production,N'ShorttermPensionContributionAccruals',@ERCA,@ETB,			-1,		+1870,	1870,		NULL,							NULL,			NULL,				@MesfinWolde,			NULL),
(	13,	3,	@Production,N'ShorttermEmployeeBenefitsAccruals',@MesfinWolde,@Basic,	-1,		+1,		15000,		NULL,							NULL,			NULL,				NULL,					NULL),
(	13,	4,	@Production,N'ShorttermEmployeeBenefitsAccruals',@MesfinWolde,@Transportation,-1,+1,	2000,		NULL,							NULL,			NULL,				NULL,					NULL);
INSERT INTO @ESave -- Overtime and Costing
([LineIndex],EntryNumber,OperationId,AccountId,		AgentId,		ResourceId,	Direction, Amount,	[Value],	RelatedResourceId,				[Reference],	[RelatedReference], [RelatedAgentId], [RelatedAmount]) VALUES
-- Feb 2018 Overtime: recorded while taken
(	14,	1,	@Production,N'OtherInventories',		@Production,@Labor,		+1,		+20,	3000,		NULL,							NULL,			NULL,				NULL,					NULL),
(	14,	2,	@Production,N'ShorttermEmployeeBenefitsAccruals',@MesfinWolde,@HOvertime,-1,	+20,	3000,		NULL,							NULL,			NULL,				NULL,					NULL),
-- Feb 2018 Job 1 Hours Logging
(	15, 1,	@Production,N'WorkInProgress',			@Production,@TeddyBear,	+1,		2,		1000,		@Cotton,						'JO01',			NULL,				NULL,					1000),
(	15, 2,	@Production,N'RawMaterials',			@Production,@Cotton,		-1,		+1,		1000,		NULL,							NULL,			NULL,				NULL,					NULL),
(	15, 3,	@Production,N'WorkInProgress',			@Production,@TeddyBear,	+1,		0,		5000,		@Labor,							'JO01',			NULL,				@MesfinWolde,			50),
(	15, 4,	@Production,N'OtherInventories',		@Production,@Labor,		-1,		+50,	5000,		NULL,							NULL,			NULL,				@MesfinWolde,			NULL);
INSERT INTO @ESave -- Payroll
([LineIndex],EntryNumber,OperationId,AccountId,		AgentId,		ResourceId,	Direction, Amount,	[Value],	NoteId,							[Reference],	[RelatedReference], [RelatedAgentId], [RelatedAmount]) VALUES
-- Feb 2018 Paysheet: Invoicing for basic salary
(	16,	1,	@Production,N'ShorttermEmployeeBenefitsAccruals',@MesfinWolde,@Basic,	+1,		+1,		15000,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),
(	16,	2,	@Production,N'CurrentPayablesToEmployees',@MesfinWolde,	@ETB,			-1,		15000,	NULL,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),
-- Feb 2018 Paysheet: Invoicing for transportation
(	17,	1,	@Production,N'ShorttermEmployeeBenefitsAccruals',@MesfinWolde,@Transportation,+1,+1,	2000,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),
(	17,	2,	@Production,N'CurrentPayablesToEmployees',@MesfinWolde,	@ETB,			-1,		2000,	NULL,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),
-- Feb 2018 Paysheet: Invoicing for overtime
(	18,	1,	@Production,N'ShorttermEmployeeBenefitsAccruals',@MesfinWolde,@HOvertime,+1,	+20,	3000,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),
(	18,	2,	@Production,N'CurrentPayablesToEmployees',@MesfinWolde,	@ETB,			-1,		3000,	NULL,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),
-- Feb 2018 Paysheet: Withholding Income Tax
(	19,	1,	@Production,N'CurrentPayablesToEmployees',@MesfinWolde,	@ETB,			+1,		4450,	NULL,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),
(	19,	2,	@Production,N'CurrentEmployeeIncomeTaxPayable',@ERCA,	@ETB,			-1,		4450,	NULL,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),
-- Feb 2018 Paysheet: Withholding Social security
(	20,	1,	@Production,N'CurrentPayablesToEmployees',@MesfinWolde,	@ETB,			+1,		1190,	NULL,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),
(	20,	2,	@Production,N'ShorttermPensionContributionAccruals',@ERCA,@ETB,			+1,		1870,	NULL,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),
(	20,	3,	@Production,N'CurrentSocialSecurityTaxPayable',@ERCA,	@ETB,			-1,	1870+1190,	NULL,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),
-- Feb 2018 Paysheet: Loan Deduction
(	21,	1,	@Production,N'CurrentPayablesToEmployees',@MesfinWolde,	@ETB,			+1,		2000,	NULL,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),
(	21,	2,	@Production,N'OtherCurrentReceivables',	@MesfinWolde,	@ETB,			-1,		2000,	NULL,		NULL,							'2018.02',		NULL,				@MesfinWolde,			NULL),

-- Feb 2018 Salaries Xfer: Similar to Rental payment. We can deduct loans, and cost sharing before payments
(	22,	1,	@Production,N'CurrentPayablesToEmployees',@MesfinWolde,	@ETB,			+1,		12360,	NULL,		NULL,							NULL,			NULL,				NULL,					NULL),
(	22,	2,	@Production,N'BalancesWithBanks',		@CBEETB,		@ETB,			-1,		12360,	NULL,		N'PaymentsToAndOnBehalfOfEmployees',N'Ck004',	NULL,				NULL,					NULL);
*/