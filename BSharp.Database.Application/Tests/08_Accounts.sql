/* Use Cases
Missing
	- Inserting
	- Updating
	- Deleting
	- Activating
	- Deactivating
*/
-- TODO
-- Try Accounts that have built in Resource, Agent, Responsibility center, or a combination thereof.

DECLARE @CBEUSD INT, @CBEETB INT, @CBELC INT, @ESL INT, @CapitalMA INT, @CapitalAA INT, 
	@RegusAccount INT, @VimeksAccount INT, @NocJimmaAccount INT, @ToyotaAccount INT, @PrepaidRental INT;
DECLARE @PPEVehicles INT, @PPEWarehouse INT;
DECLARE @fuelHR INT, @fuelSalesAdminAG INT, @fuelProduction INT, @fuelSalesDistAG INT;
DECLARE @VATInput INT, @VATOutput INT, @SalariesAdmin INT, @SalariesAccrualsTaxable INT, @OvertimeAdmin INT,
		@SalariesAccrualsNonTaxable INT, @EmployeesPayable INT, @EmployeesIncomeTaxPayable INT;

INSERT INTO dbo.Accounts([Name], [Code], [IfrsAccountClassificationId]) VALUES
(N'CBE - USD', N'1101', N'BalancesWithBanks'),
(N'CBE - ETB', N'1102', N'BalancesWithBanks'), -- reserved money to pay for LC when needed
(N'CBE - LC', N'1201', N'TradeAndOtherCurrentReceivables'), -- reserved money to pay for LC when needed

(N'TF1903950009', N'1209', N'CurrentInventoriesInTransit'), -- Merchandise in transit, for given LC
(N'PPE Warehouse', N'1210', N'OtherInventories'),
(N'PPE - Vehicles', N'1301', N'Vehicles'),

(N'Vimeks', N'2101', N'TradeAndOtherCurrentPayablesToTradeSuppliers'),

(N'Capital - MA', N'3101', N'IssuedCapital'),
(N'Capital - AA', N'3102', N'IssuedCapital');
SELECT @CBEUSD = [Id] FROM dbo.Accounts WHERE Code = N'1101';
SELECT @CBEETB = [Id] FROM dbo.Accounts WHERE Code = N'1102';
SELECT @CBELC = [Id] FROM dbo.Accounts WHERE Code = N'1201';
SELECT @ESL = [Id] FROM dbo.Accounts WHERE Code = N'1209';
SELECT @PPEWarehouse = [Id] FROM dbo.Accounts WHERE Code = N'1210';
SELECT @PPEVehicles = [Id] FROM dbo.Accounts WHERE Code = N'1301'; 
SELECT @VimeksAccount = [Id] FROM dbo.Accounts WHERE Code = N'2101';
SELECT @CapitalMA = [Id] FROM dbo.Accounts WHERE Code = N'3101';
SELECT @CapitalAA = [Id] FROM dbo.Accounts WHERE Code = N'3102';
INSERT INTO dbo.AccountsResources(AccountId, ResourceId) VALUES
(@CBEUSD, @USD),
(@VimeksAccount, @USD);


INSERT INTO dbo.Accounts
([Name],				 [Code], [IfrsAccountClassificationId]) VALUES
(N'Noc Jimma',			N'2102',  N'TradeAndOtherCurrentPayablesToTradeSuppliers'),
(N'Toyota',				N'2103',  N'TradeAndOtherCurrentPayablesToTradeSuppliers'),
(N'Regus',				N'2104',  N'TradeAndOtherCurrentPayablesToTradeSuppliers'),
(N'Prepaid Rental',		N'1501',  N'CurrentPrepaidExpenses'),
(N'VAT Input',			N'1401',  N'CurrentValueAddedTaxReceivables'),
(N'VAT Output',			N'2401',  N'CurrentValueAddedTaxPayables')
;
SELECT @NocJimmaAccount = [Id] FROM dbo.Accounts WHERE Code = N'2102';
SELECT @ToyotaAccount = [Id] FROM dbo.Accounts WHERE Code = N'2103';
SELECT @RegusAccount = [Id] FROM dbo.Accounts WHERE Code = N'2104';
SELECT @PrepaidRental = [Id] FROM dbo.Accounts WHERE Code = N'1501';
SELECT @VATInput = [Id] FROM dbo.Accounts WHERE Code = N'1401';
SELECT @VATOutput = [Id] FROM dbo.Accounts WHERE Code = N'2401';
INSERT INTO dbo.AccountsAgents([AccountId], [AgentId]) VALUES
(@NocJimmaAccount, @NocJimma),
(@ToyotaAccount, @Toyota),
(@RegusAccount, @Regus),
(@PrepaidRental, @Regus),
(@VATInput, @ERCA),
(@VATOutput, @ERCA);

INSERT INTO dbo.Accounts
([Name],							[Code],	[IfrsAccountClassificationId]) VALUES
(N'Salaries Accruals, taxable',		N'2501',  N'ShorttermEmployeeBenefitsAccruals'),
(N'Salaries Accruals, non taxable',	N'2502',  N'ShorttermEmployeeBenefitsAccruals'),
(N'Employees payable',				N'2503',  N'ShorttermEmployeeBenefitsAccruals'),
(N'Employees Income Tax payable',	N'2504',  N'CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax')
;
SELECT @SalariesAccrualsTaxable = [Id] FROM dbo.Accounts WHERE Code = N'2501';
SELECT @SalariesAccrualsNonTaxable = [Id] FROM dbo.Accounts WHERE Code = N'2502';
SELECT @EmployeesPayable = [Id] FROM dbo.Accounts WHERE Code = N'2503';
SELECT @EmployeesIncomeTaxPayable = [Id] FROM dbo.Accounts WHERE Code = N'2504'
INSERT INTO dbo.AccountsAgents([AccountId], [AgentId]) VALUES
(@SalariesAccrualsTaxable, @Mestawet),
(@SalariesAccrualsNonTaxable, @Mestawet),
(@EmployeesPayable, @Mestawet);

INSERT INTO dbo.Accounts([Name], [Code], [IfrsAccountClassificationId], [IfrsEntryClassificationId]) VALUES
(N'fuel - HR', N'5101', N'AdministrativeExpense', N'TransportationExpense'),
(N'fuel - Sales - admin - AG', N'5102', N'AdministrativeExpense', N'TransportationExpense'),
(N'fuel - Production', N'5103', N'AdministrativeExpense', N'TransportationExpense'),
(N'fuel - Sales - distribution - AG', N'5201', N'DistributionCosts', N'TransportationExpense');
SELECT @fuelHR = [Id] FROM dbo.Accounts WHERE Code = N'5101';
SELECT @fuelSalesAdminAG = [Id] FROM dbo.Accounts WHERE Code = N'5102';
SELECT @fuelProduction = [Id] FROM dbo.Accounts WHERE Code = N'5103';
SELECT @fuelSalesDistAG = [Id] FROM dbo.Accounts WHERE Code = N'5201';
INSERT INTO dbo.AccountsResponsibilityCenters([AccountId], [ResponsibilityCenterId]) VALUES
(@fuelHR, @HROps),
(@fuelSalesAdminAG, @SalesOpsAG),
(@fuelProduction, @ProductionOps),
(@fuelSalesDistAG, @SalesOpsAG);

INSERT INTO dbo.Accounts([Name], [Code], [IfrsAccountClassificationId], [IfrsEntryClassificationId]) VALUES
(N'Salaries - Admin', N'5202', N'AdministrativeExpense', N'WagesAndSalaries'),
(N'Overtime - Admin', N'5203', N'AdministrativeExpense', N'WagesAndSalaries')
;
SELECT @SalariesAdmin = [Id] FROM dbo.Accounts WHERE Code = N'5202';
SELECT @OvertimeAdmin = [Id] FROM dbo.Accounts WHERE Code = N'5203';