/* Use Cases
Missing
	- Inserting
	- Updating
	- Deleting
	- Activating
	- Deactivating
*/
DECLARE @CBEUSD INT, @CBELC INT, @ESL INT, @CapitalMA INT, @CapitalAA INT, @VimeksAccount INT, @NocJimmaAccount INT;
DECLARE @fuelHR INT, @fuelSalesAdminAG INT, @fuelProduction INT, @fuelSalesDistAG INT;

INSERT INTO dbo.Accounts([Name], [Code], [IfrsAccountClassificationId]) VALUES
(N'CBE - USD', N'1101', N'BalancesWithBanks'),
(N'CBE - LC', N'1102', N'BalancesWithBanks'), -- reserved money to pay for LC when needed
(N'TF1903950009', N'1209', N'CurrentInventoriesInTransit'), -- Merchandise in transit, for given LC

(N'Vimeks', N'2101', N'TradeAndOtherCurrentPayablesToTradeSuppliers'),

(N'Capital - MA', N'3101', N'IssuedCapital'),
(N'Capital - AA', N'3102', N'IssuedCapital');

INSERT INTO dbo.Accounts([Name], [Code], [IfrsAccountClassificationId], [AgentId]) VALUES
(N'Noc Jimma', N'2102',  N'TradeAndOtherCurrentPayablesToTradeSuppliers', @NocJimma);

INSERT INTO dbo.Accounts([Name], [Code], [IfrsAccountClassificationId], [DefaultDebitIfrsEntryClassificationId],
 [DefaultCreditIfrsEntryClassificationId], [ResponsibilityCenterId]) VALUES
(N'fuel - HR', N'5101', N'AdministrativeExpense', N'TransportationExpense', N'TransportationExpense',@HROps),
(N'fuel - Sales - admin - AG', N'5102', N'AdministrativeExpense', N'TransportationExpense', N'TransportationExpense', @SalesOpsAG),
(N'fuel - Production', N'5103', N'AdministrativeExpense', N'TransportationExpense', N'TransportationExpense', @ProductionOps),
(N'fuel - Sales - distribution - AG', N'5201', N'DistributionCosts', N'TransportationExpense', N'TransportationExpense', @SalesOpsAG)
;

SELECT @CBEUSD = [Id] FROM dbo.Accounts WHERE Code = N'1101';
SELECT @CBELC = [Id] FROM dbo.Accounts WHERE Code = N'1102';
SELECT @ESL =  [Id] FROM dbo.Accounts WHERE Code = N'1209';
SELECT @VimeksAccount =  [Id] FROM dbo.Accounts WHERE Code = N'2101';
SELECT @NocJimmaAccount =  [Id] FROM dbo.Accounts WHERE Code = N'2102';

SELECT @CapitalMA = [Id] FROM dbo.Accounts WHERE Code = N'3101';
SELECT @CapitalAA = [Id] FROM dbo.Accounts WHERE Code = N'3102';

SELECT @fuelHR = [Id] FROM dbo.Accounts WHERE Code = N'5101';
SELECT @fuelSalesAdminAG = [Id] FROM dbo.Accounts WHERE Code = N'5102';
SELECT @fuelProduction = [Id] FROM dbo.Accounts WHERE Code = N'5103';
SELECT @fuelSalesDistAG = [Id] FROM dbo.Accounts WHERE Code = N'5201';