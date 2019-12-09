DECLARE @SmartAccounts dbo.AccountList;
																														--[ResponsibilityCenterId]		INT,
																														--[ContractType]					NVARCHAR (50),--	REFERENCES dbo.[ContractTypes]([Id]),
																														--[AgentDefinitionId]				NVARCHAR (50),
																														--[ResourceClassificationId]		INT,
																														--[IsCurrent]						BIT,
																														--[AgentId]						INT,
																														--[ResourceId]					INT,
																														--[Identifier]					NVARCHAR (10),
																														--[EntryClassificationId]			INT
--DECLARE @FunctionalETB
--INSERT INTO @SmartAccounts([Index], [IsSmart],
--	[AccountTypeId],		[AccountClassificationId],	[Name],								[Code],		[ContractType], [AgentDefinitionId], [ResourceClassificationId], [IsCurrent], [AgentId],	[ResourceId]) VALUES
----(0,N'Cash',				@BankAndCash_AC,			N'CBE - USD',						N'1101'),
----(1,N'Cash',				@BankAndCash_AC,			N'CBE - ETB',						N'1102'),
--(0,1,N'Cash',				@BankAndCash_AC,			N'CBE - USD',						N'1103',	N'OnHand',		N'banks',			dbo.fn_RCCode__Id(N'Cash'),		+1,			@Bank_CBE,	@),
--(1,1,N'Cash',				@BankAndCash_AC,			N'CBE - ETB',						N'1104',	N'OnHand',		N'banks',			dbo.fn_RCCode__Id(N'Cash')),	+1,			@Bank_CBE);
--(3,1,N'Inventory',			@Inventories_AC,			N'TF1903950009',					N'1209'), -- Merchandise in transit, for given LC
--(4,1,N'Inventory',			@Inventories_AC,			N'PPE Warehouse',					N'1210'),
--(5,1,N'FixedAssets',		@NonCurrentAssets_AC,		N'PPE - Vehicles',					N'1301'),
--(6,1,N'OtherCurrentAssets',	@Debtors_AC,				N'Prepaid Rental',					N'1401'),
--(7,1,N'AccountsReceivable',	@Debtors_AC,				N'VAT Input',						N'1501'),
--(8,1,N'AccountsPayable',	@Liabilities_AC,			N'Vimeks',							N'2101'),
--(9,1,N'AccountsPayable',	@Liabilities_AC,			N'Noc Jimma',						N'2102'),
--(10,1,N'AccountsPayable',	@Liabilities_AC,			N'Toyota',							N'2103'),
--(11,1,N'AccountsPayable',	@Liabilities_AC,			N'Regus',							N'2104'),
--(12,1,N'AccountsPayable',	@Liabilities_AC,			N'Salaries Accruals, taxable',		N'2501'),
--(13,1,N'AccountsPayable',	@Liabilities_AC,			N'Salaries Accruals, non taxable',	N'2502'),
--(14,1,N'AccountsPayable',	@Liabilities_AC,			N'Employees payable',				N'2503'),
--(17,1,N'EquityDoesntClose',	@Equity_AC,					N'Capital - MA',					N'3101'),
--(18,1,N'EquityDoesntClose',	@Equity_AC,					N'Capital - AA',					N'3102'),
--(19,1,N'Expenses',			@Expenses_AC,				N'fuel - HR',						N'5101'),
--(20,1,N'Expenses',			@Expenses_AC,				N'fuel - Sales - admin - AG',		N'5102'),
--(21,1,N'CostofSales',		@Expenses_AC,				N'fuel - Production',				N'5103'),
--(22,1,N'Expenses',			@Expenses_AC,				N'fuel - Sales - distribution - AG',1,N'5201'),
--(23,1,N'Expenses',			@Expenses_AC,				N'Salaries - Admin',				N'5202'),
--(24,1,N'Expenses',			@Expenses_AC,				N'Overtime - Admin',				N'5203');

--EXEC [api].[Accounts__Save] --  N'cash-and-cash-equivalents',
--	@Entities = @SmartAccounts,
--	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--IF @ValidationErrorsJson IS NOT NULL 
--BEGIN
--	Print 'Inserting Smart Accounts'
--	GOTO Err_Label;
--END;

--IF @DebugAccounts = 1
--	SELECT * FROM map.Accounts();

--SELECT @CBEUSD = [Id] FROM dbo.[Accounts] WHERE Code = N'1101';
--SELECT @CBEETB = [Id] FROM dbo.[Accounts] WHERE Code = N'1102';
--SELECT @CBELC = [Id] FROM dbo.[Accounts] WHERE Code = N'1201';
--SELECT @ESL = [Id] FROM dbo.[Accounts] WHERE Code = N'1209';
--SELECT @PPEWarehouse = [Id] FROM dbo.[Accounts] WHERE Code = N'1210';
--SELECT @PPEVehicles = [Id] FROM dbo.[Accounts] WHERE Code = N'1301'; 
--SELECT @PrepaidRental = [Id] FROM dbo.[Accounts] WHERE Code = N'1401';
--SELECT @VATInput = [Id] FROM dbo.[Accounts] WHERE Code = N'1501';

--SELECT @VimeksAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2101';
--SELECT @CapitalMA = [Id] FROM dbo.[Accounts] WHERE Code = N'3101';
--SELECT @CapitalAA = [Id] FROM dbo.[Accounts] WHERE Code = N'3102';

--SELECT @NocJimmaAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2102';
--SELECT @ToyotaAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2103';
--SELECT @RegusAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2104';
--SELECT @SalariesAccrualsTaxable = [Id] FROM dbo.[Accounts] WHERE Code = N'2501';
--SELECT @SalariesAccrualsNonTaxable = [Id] FROM dbo.[Accounts] WHERE Code = N'2502';
--SELECT @EmployeesPayable = [Id] FROM dbo.[Accounts] WHERE Code = N'2503';
--SELECT @VATOutput = [Id] FROM dbo.[Accounts] WHERE Code = N'2601';
--SELECT @EmployeesIncomeTaxPayable = [Id] FROM dbo.[Accounts] WHERE Code = N'2602';

--SELECT @fuelHR = [Id] FROM dbo.[Accounts] WHERE Code = N'5101';
--SELECT @fuelSalesAdminAG = [Id] FROM dbo.[Accounts] WHERE Code = N'5102';
--SELECT @fuelProduction = [Id] FROM dbo.[Accounts] WHERE Code = N'5103';
--SELECT @fuelSalesDistAG = [Id] FROM dbo.[Accounts] WHERE Code = N'5201';

--SELECT @SalariesAdmin = [Id] FROM dbo.[Accounts] WHERE Code = N'5202';
--SELECT @OvertimeAdmin = [Id] FROM dbo.[Accounts] WHERE Code = N'5203';
