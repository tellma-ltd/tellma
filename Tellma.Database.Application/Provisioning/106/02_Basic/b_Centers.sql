INSERT INTO @Centers([Index],[ParentIndex], [Name],[Name2],[Code], [CenterType]) VALUES
(-2,NULL, N'Soreti', N'ሶሬቲ', N'', N'Segment'),
(-1,-2, N'Soreti - Shared', N'ሶሬቲ - የተጋራ', N'0', N'Abstract'),
(0,-1, N'Soreti - Segment', N'ሶሬቲ - ያልተገለጸ', N'0000', N'Common'),
(300,-1, N'Soreti - Marketing', N'ሶሬቲ - ማርኬቲንግ', N'0300', N'DistributionCosts'),
(4,-1, N'Soreti - Administration', N'ሶሬቲ - አስተዳደር', N'04', N'Abstract'),
(400,4, N'General', N'ጠቅላላ', N'0400', N'AdministrativeExpense'),
(410,4, N'Management', N'አስተዳደር', N'0410', N'AdministrativeExpense'),
(420,4, N'Finance', N'የመንግሥት ገንዘብ አስተዳደር', N'0420', N'AdministrativeExpense'),
(430,4, N'IT', N'የአይቲ', N'0430', N'AdministrativeExpense'),
(5,-1, N'Soreti - Shared Sites', N'ሶሬቲ - የተጋሩ ጣቢያዎች', N'05', N'Abstract'),
(510,5, N'Adama site 1', N'አዳማ ጣቢያ 1', N'0510', N'ProductionExtension'),
(520,5, N'Adama site 2', N'አዳማ ጣቢያ 2', N'0520', N'ProductionExtension'),
(1,-2, N'Trading Centers', N'ትሬዲንግ ማዕከል', N'1', N'Abstract'),
(1000,1, N'Trading - Unallocated', N'ትሬዲንግ - unallocated', N'1000', N'Unallocated'),
(11,1, N'Export', N'ወደ ውጭ የተላኩ እህሎች', N'11', N'Abstract'),
(1100,11, N'Export - Unallocated', N'ወደ ውጪ ላክ - unallocated', N'1100', N'Unallocated'),
(1110,11, N'Export - Sales', N'ላክ - ሽያጭ', N'1110', N'Revenue'),
(1120,11, N'Export - Cost of Sales', N'ላክ - ሽያጭ ዋጋ', N'1120', N'CostOfSales'),
(1130,11, N'Export - Distribution', N'ወደ ውጪ ላክ - S & D', N'1130', N'DistributionCosts'),
(1140,11, N'Export - Administration', N'ላክ - የአስተዳዳሪ', N'1140', N'AdministrativeExpense'),
(115,11, N'Export - Production', N'ላክ - ፕሮዳክሽን', N'115', N'Abstract'),
(1151,115, N'Export Factory - Site 1', N'የውጭ ንግድ ፋብሪካ - ጣቢያ 1', N'1151', N'ProductionExtension'),
(1152,115, N'Export Factory - Site 2', N'የውጭ ንግድ ፋብሪካ - የጣቢያ 2', N'1152', N'ProductionExtension'),
(116,11, N'Export - Warehouses', N'ላክ - መጋዘኖችን', N'116', N'Abstract'),
(1161,116, N'Export Warehouses - Site 1', N'ወደ ውጪ ላክ መጋዘኖችን - የጣቢያ 1', N'1161', N'Inventories'),
(1162,116, N'Export Warehouses - Site 2', N'ወደ ውጪ ላክ መጋዘኖችን - የጣቢያ 2', N'1162', N'Inventories'),
(12,1, N'Import', N'አስገባ', N'12', N'Abstract'),
(1200,12, N'Import - Unallocated', N'አስመጣ - unallocated', N'1200', N'Unallocated'),
(1210,12, N'Import - Sales', N'አስመጣ - ሽያጭ', N'1210', N'Revenue'),
(1220,12, N'Import - Cost of sales', N'አስመጣ - የሽያጭ ዋጋ', N'1220', N'CostOfSales'),
(1230,12, N'Import - Distribution', N'አስመጣ - S & D', N'1230', N'DistributionCosts'),
(1240,12, N'Import - Administration', N'አስመጣ - የአስተዳዳሪ', N'1240', N'AdministrativeExpense'),
(1260,12, N'Import - Warehouses', N'አስመጣ - መጋዘኖችን', N'1260', N'Inventories'),
(1270,12, N'Import - in transit', N'አስመጣ - በሽግግር ላይ', N'1270', N'CurrentInventoriesInTransit'),
(13,1, N'Agro Processing', N'አግሮ በመስራት ላይ', N'13', N'Abstract'),
(1300,13, N'Agro Processing - Unallocated', N'አግሮ በመስራት ላይ - unallocated', N'1300', N'Unallocated'),
(1310,13, N'Agro Processing - Sales', N'አግሮ በመስራት ላይ - ሽያጭ', N'1310', N'Revenue'),
(1320,13, N'Agro Processing - Cost of Sales', N'አግሮ በመስራት ላይ - የሽያጭ ዋጋ', N'1320', N'CostOfSales'),
(1330,13, N'Agro Processing - Distribution', N'አግሮ በመስራት ላይ - S & D', N'1330', N'DistributionCosts'),
(1340,13, N'Agro Processing - Administration', N'አግሮ በመስራት ላይ - የአስተዳዳሪ', N'1340', N'AdministrativeExpense'),
(1350,13, N'Agro Processing - Production', N'አግሮ በመስራት ላይ - ፕሮዳክሽን', N'1350', N'ProductionExtension'),
(1360,12, N'Agro Processing- Warehouses', N'አግሮ Processing- መጋዘኖችን', N'1360', N'Inventories'),
(14,1, N'Manufacturing', N'ማኑፋክቸሪንግ', N'14', N'Abstract'),
(1400,12, N'Manufacturing - Unallocated', N'ማኑፋክቸሪንግ - unallocated', N'1400', N'Unallocated'),
(1410,12, N'Manufacturing - Sales', N'ማኑፋክቸሪንግ - ሽያጭ', N'1410', N'Revenue'),
(1420,12, N'Manufacturing - Cost of sales', N'ማኑፋክቸሪንግ - የሽያጭ ዋጋ', N'1420', N'CostOfSales'),
(1430,12, N'Manufacturing - Distribution', N'ማኑፋክቸሪንግ - S & D', N'1430', N'DistributionCosts'),
(1440,12, N'Manufacturing - Administration', N'ማኑፋክቸሪንግ - የአስተዳዳሪ', N'1440', N'AdministrativeExpense'),
(1450,12, N'Manufacturing - Production', N'ማኑፋክቸሪንግ - ፕሮዳክሽን', N'1450', N'ProductionExtension'),
(1460,12, N'Manufacturing - Warehouses', N'ማኑፋክቸሪንግ - መጋዘኖችን', N'1460', N'Inventories'),
(1470,12, N'Manufacturing - in transit', N'በማምረቻ - በሽግግር ላይ', N'1470', N'CurrentInventoriesInTransit'),
(15,1, N'Local Trade', N'ወደ ውጭ የተላኩ እህሎች', N'15', N'Abstract'),
(1500,11, N'Local Trade - Unallocated', N'አካባቢያዊ የንግድ - unallocated', N'1500', N'Unallocated'),
(1510,11, N'Local Trade - Sales', N'አካባቢያዊ የንግድ - ሽያጭ', N'1510', N'Revenue'),
(1520,11, N'Local Trade - Cost of Sales', N'አካባቢያዊ ንግድ - የሽያጭ ዋጋ', N'1520', N'CostOfSales'),
(1530,11, N'Local Trade - Distribution', N'አካባቢያዊ ንግድ - S & D', N'1530', N'DistributionCosts'),
(1540,11, N'Local Trade - Administration', N'አካባቢያዊ ንግድ - የአስተዳዳሪ', N'1540', N'AdministrativeExpense'),
(155,11, N'Local Trade - Production', N'አካባቢያዊ ንግድ - ፕሮዳክሽን', N'155', N'Abstract'),
(1551,115, N'Local Trade Factory - Site 1', N'አካባቢያዊ የንግድ ፋብሪካ - ጣቢያ 1', N'1551', N'ProductionExtension'),
(1552,115, N'Local Trade Factory - Site 2', N'አካባቢያዊ የንግድ ፋብሪካ - የጣቢያ 2', N'1552', N'ProductionExtension'),
(156,11, N'Local Trade - Warehouses', N'አካባቢያዊ የንግድ - መጋዘኖችን', N'156', N'Abstract'),
(1561,116, N'Local Trade Warehouses - Site 1', N'አካባቢያዊ ንግድ መጋዘኖችን - የጣቢያ 1', N'1561', N'Inventories'),
(1562,116, N'Local Trade Warehouses - Site 2', N'አካባቢያዊ ንግድ መጋዘኖችን - የጣቢያ 2', N'1562', N'Inventories'),
(2,0, N'Real Estate Centers', N'ሪል እስቴት ማዕከል', N'2', N'Abstract'),
(21,2, N'Soreti Mall', N'ሶሬቲ ሜል', N'21', N'Abstract'),
(2100,21, N'Soreti Mall - Unallocated', N'ሶሬቲ ሜል - unallocated', N'2100', N'Unallocated'),
(2110,21, N'Soreti Mall - Sales', N'ሶሬቲ ሜል - ሽያጭ', N'2110', N'Revenue'),
(2120,21, N'Soreti Mall - Cost of sales', N'ሶሬቲ ሜል - የሽያጭ ዋጋ', N'2120', N'CostOfSales'),
(2130,21, N'Soreti Mall - Distribution', N'ሶሬቲ ሜል - S & D', N'2130', N'DistributionCosts'),
(2140,21, N'Soreti Mall - Administration', N'ሶሬቲ ሜል - የአስተዳዳሪ', N'2140', N'AdministrativeExpense'),
(220,200, N'AA Building', N'አዲስ አበባ ብሩንዲ', N'22', N'Abstract'),
(2200,21, N'AA Building - Unallocated', N'AA ህንጻ - unallocated', N'2200', N'Unallocated'),
(2210,21, N'AA Building - Sales', N'AA ህንጻ - ሽያጭ', N'2210', N'Revenue'),
(2220,21, N'AA Building - Cost of sales', N'AA ህንጻ - የሽያጭ ዋጋ', N'2220', N'CostOfSales'),
(2230,21, N'AA Building - Distribution', N'AA ሕንፃ - S & D', N'2230', N'DistributionCosts'),
(2240,21, N'AA Building - Administration', N'AA ህንጻ - የአስተዳዳሪ', N'2240', N'AdministrativeExpense');

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Declarations
DECLARE @106C_Soreti INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti');
DECLARE @106C_SoretiShared INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti - Shared');
DECLARE @106C_SoretiSegment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti - Segment');
DECLARE @106C_SoretiMarketing INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti - Marketing');
DECLARE @106C_SoretiAdministration INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti - Administration');
DECLARE @106C_General INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'General');
DECLARE @106C_Management INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Management');
DECLARE @106C_Finance INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Finance');
DECLARE @106C_IT INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'IT');
DECLARE @106C_SoretiSharedSites INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti - Shared Sites');
DECLARE @106C_Adamasite1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Adama site 1');
DECLARE @106C_Adamasite2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Adama site 2');
DECLARE @106C_TradingCenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Trading Centers');
DECLARE @106C_TradingUnallocated INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Trading - Unallocated');
DECLARE @106C_Export INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export');
DECLARE @106C_ExportUnallocated INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export - Unallocated');
DECLARE @106C_ExportSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export - Sales');
DECLARE @106C_ExportCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export - Cost of Sales');
DECLARE @106C_ExportDistribution INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export - Distribution');
DECLARE @106C_ExportAdministration INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export - Administration');
DECLARE @106C_ExportProduction INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export - Production');
DECLARE @106C_ExportFactorySite1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export Factory - Site 1');
DECLARE @106C_ExportFactorySite2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export Factory - Site 2');
DECLARE @106C_ExportWarehouses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export - Warehouses');
DECLARE @106C_ExportWarehousesSite1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export Warehouses - Site 1');
DECLARE @106C_ExportWarehousesSite2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export Warehouses - Site 2');
DECLARE @106C_Import INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Import');
DECLARE @106C_ImportUnallocated INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Import - Unallocated');
DECLARE @106C_ImportSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Import - Sales');
DECLARE @106C_ImportCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Import - Cost of sales');
DECLARE @106C_ImportDistribution INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Import - Distribution');
DECLARE @106C_ImportAdministration INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Import - Administration');
DECLARE @106C_ImportWarehouses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Import - Warehouses');
DECLARE @106C_Importintransit INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Import - in transit');
DECLARE @106C_AgroProcessing INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Agro Processing');
DECLARE @106C_AgroProcessingUnallocated INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Agro Processing - Unallocated');
DECLARE @106C_AgroProcessingSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Agro Processing - Sales');
DECLARE @106C_AgroProcessingCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Agro Processing - Cost of Sales');
DECLARE @106C_AgroProcessingDistribution INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Agro Processing - Distribution');
DECLARE @106C_AgroProcessingAdministration INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Agro Processing - Administration');
DECLARE @106C_AgroProcessingProduction INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Agro Processing - Production');
DECLARE @106C_AgroProcessingWarehouses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Agro Processing- Warehouses');
DECLARE @106C_Manufacturing INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Manufacturing');
DECLARE @106C_ManufacturingUnallocated INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Manufacturing - Unallocated');
DECLARE @106C_ManufacturingSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Manufacturing - Sales');
DECLARE @106C_ManufacturingCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Manufacturing - Cost of sales');
DECLARE @106C_ManufacturingDistribution INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Manufacturing - Distribution');
DECLARE @106C_ManufacturingAdministration INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Manufacturing - Administration');
DECLARE @106C_ManufacturingProduction INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Manufacturing - Production');
DECLARE @106C_ManufacturingWarehouses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Manufacturing - Warehouses');
DECLARE @106C_Manufacturingintransit INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Manufacturing - in transit');
DECLARE @106C_LocalTrade INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Trade');
DECLARE @106C_LocalTradeUnallocated INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Trade - Unallocated');
DECLARE @106C_LocalTradeSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Trade - Sales');
DECLARE @106C_LocalTradeCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Trade - Cost of Sales');
DECLARE @106C_LocalTradeDistribution INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Trade - Distribution');
DECLARE @106C_LocalTradeAdministration INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Trade - Administration');
DECLARE @106C_LocalTradeProduction INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Trade - Production');
DECLARE @106C_LocalTradeFactorySite1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Trade Factory - Site 1');
DECLARE @106C_LocalTradeFactorySite2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Trade Factory - Site 2');
DECLARE @106C_LocalTradeWarehouses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Trade - Warehouses');
DECLARE @106C_LocalTradeWarehousesSite1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Trade Warehouses - Site 1');
DECLARE @106C_LocalTradeWarehousesSite2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Trade Warehouses - Site 2');
DECLARE @106C_RealEstateCenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Real Estate Centers');
DECLARE @106C_SoretiMall INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti Mall');
DECLARE @106C_SoretiMallUnallocated INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti Mall - Unallocated');
DECLARE @106C_SoretiMallSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti Mall - Sales');
DECLARE @106C_SoretiMallCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti Mall - Cost of sales');
DECLARE @106C_SoretiMallDistribution INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti Mall - Distribution');
DECLARE @106C_SoretiMallAdministration INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti Mall - Administration');
DECLARE @106C_AABuilding INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'AA Building');
DECLARE @106C_AABuildingUnallocated INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'AA Building - Unallocated');
DECLARE @106C_AABuildingSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'AA Building - Sales');
DECLARE @106C_AABuildingCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'AA Building - Cost of sales');
DECLARE @106C_AABuildingDistribution INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'AA Building - Distribution');
DECLARE @106C_AABuildingAdministration INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'AA Building - Administration');