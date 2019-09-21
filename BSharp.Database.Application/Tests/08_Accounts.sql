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
DECLARE @VATInput INT, @VATOutput INT--, @PurchaseInvoices INT, @SalesInvoices INT;

INSERT INTO dbo.Accounts([Name], [Code], [IfrsAccountClassificationId], [IsMultiResource], [ResourceId]) VALUES
(N'CBE - USD', N'1101', N'BalancesWithBanks', 0, @USD),
(N'CBE - ETB', N'1102', N'BalancesWithBanks', 0, NULL), -- reserved money to pay for LC when needed
(N'CBE - LC', N'1201', N'TradeAndOtherCurrentReceivables', 0, NULL), -- reserved money to pay for LC when needed

(N'TF1903950009', N'1209', N'CurrentInventoriesInTransit', 1, NULL), -- Merchandise in transit, for given LC
(N'PPE Warehouse', N'1210', N'OtherInventories', 0, NULL),
(N'PPE - Vehicles', N'1301', N'Vehicles', 0, NULL),

(N'Vimeks', N'2101', N'TradeAndOtherCurrentPayablesToTradeSuppliers', 0, @USD),

(N'Capital - MA', N'3101', N'IssuedCapital', 0, NULL),
(N'Capital - AA', N'3102', N'IssuedCapital', 0, NULL);

INSERT INTO dbo.Accounts
([Name],				 [Code], [IfrsAccountClassificationId],						[AgentId]) VALUES
(N'Noc Jimma',			N'2102',  N'TradeAndOtherCurrentPayablesToTradeSuppliers',	@NocJimma),
(N'Toyota',				N'2103',  N'TradeAndOtherCurrentPayablesToTradeSuppliers',	@Toyota),
(N'Regus',				N'2104',  N'TradeAndOtherCurrentPayablesToTradeSuppliers',	@Regus),
(N'Prepaid Rental',		N'1501',  N'CurrentPrepaidExpenses',						@Regus),
(N'VAT Input',			N'1401',  N'CurrentValueAddedTaxReceivables',				@ERCA),
(N'VAT Output',			N'2401',  N'CurrentValueAddedTaxPayables',					@ERCA)
;

INSERT INTO dbo.Accounts([Name], [Code], [IfrsAccountClassificationId], [IsMultiEntryClassification], [IfrsEntryClassificationId], [ResponsibilityCenterId]) VALUES
(N'fuel - HR', N'5101', N'AdministrativeExpense', 0, N'TransportationExpense', @HROps),
(N'fuel - Sales - admin - AG', N'5102', N'AdministrativeExpense', 0, N'TransportationExpense', @SalesOpsAG),
(N'fuel - Production', N'5103', N'AdministrativeExpense', 0, N'TransportationExpense', @ProductionOps),
(N'fuel - Sales - distribution - AG', N'5201', N'DistributionCosts', 0, N'TransportationExpense', @SalesOpsAG)
;

SELECT @CBEUSD = [Id] FROM dbo.Accounts WHERE Code = N'1101';
SELECT @CBEETB = [Id] FROM dbo.Accounts WHERE Code = N'1102';
SELECT @CBELC = [Id] FROM dbo.Accounts WHERE Code = N'1201';

SELECT @ESL = [Id] FROM dbo.Accounts WHERE Code = N'1209';
SELECT @PPEWarehouse = [Id] FROM dbo.Accounts WHERE Code = N'1210';
SELECT @PPEVehicles = [Id] FROM dbo.Accounts WHERE Code = N'1301'; 

SELECT @VimeksAccount = [Id] FROM dbo.Accounts WHERE Code = N'2101';
SELECT @NocJimmaAccount = [Id] FROM dbo.Accounts WHERE Code = N'2102';
SELECT @ToyotaAccount = [Id] FROM dbo.Accounts WHERE Code = N'2103';
SELECT @RegusAccount = [Id] FROM dbo.Accounts WHERE Code = N'2104';
SELECT @PrepaidRental = [Id] FROM dbo.Accounts WHERE Code = N'1501';

SELECT @CapitalMA = [Id] FROM dbo.Accounts WHERE Code = N'3101';
SELECT @CapitalAA = [Id] FROM dbo.Accounts WHERE Code = N'3102';

SELECT @fuelHR = [Id] FROM dbo.Accounts WHERE Code = N'5101';
SELECT @fuelSalesAdminAG = [Id] FROM dbo.Accounts WHERE Code = N'5102';
SELECT @fuelProduction = [Id] FROM dbo.Accounts WHERE Code = N'5103';
SELECT @fuelSalesDistAG = [Id] FROM dbo.Accounts WHERE Code = N'5201';
SELECT @VATInput = [Id] FROM dbo.Accounts WHERE Code = N'1401';
SELECT @VATOutput = [Id] FROM dbo.Accounts WHERE Code = N'2401';

--SELECT @SalesInvoices = [Id] FROM dbo.Accounts WHERE Code = N'9002';