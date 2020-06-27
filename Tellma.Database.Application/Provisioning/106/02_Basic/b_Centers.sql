INSERT INTO @Centers([Index],[ParentIndex], [Name],[Name2],[Code], [CenterType]) VALUES
(1,NULL, N'Trading Centers - HO', N'የንግድ ማዕከላት', N'1', N'Segment'),
(1000,1, N'Trading - HO', N'ትሬዲንግ', N'1000', N'Parent'),
(102,1, N'Soreti - Administration', N'ሶሬቲ - አስተዳደር', N'102', N'Abstract'),
(1021,102, N'General', N'ጠቅላላ', N'1021', N'SellingGeneralAndAdministration'),
(1022,102, N'Management', N'አስተዳደር', N'1022', N'SellingGeneralAndAdministration'),
(1023,102, N'Finance', N'ፋይናንስ', N'1023', N'SellingGeneralAndAdministration'),
(1024,102, N'IT', N'የአይቲ', N'1024', N'SellingGeneralAndAdministration'),
(1030,102, N'Soreti Admin - Shared Expenses', N'ሶሬቲ - አስተዳደር - Shared Expense', N'1030', N'SharedExpenseControl'),
(106,1, N'Grains Factories', N'እህል ፋብሪካዎች', N'106', N'Abstract'),
(1061,106, N'Grains Factories - Site 1', N'እህል ፋብሪካዎች - ጣቢያ 1', N'1061', N'ProductionExpenseControl'),
(1062,106, N'Grains Factories - Site 2', N'እህል ፋብሪካዎች - የጣቢያ 2', N'1062', N'ProductionExpenseControl'),
(11,1, N'Export', N'ወደ ውጭ የተላኩ እህሎች', N'11', N'Abstract'),
(1110,11, N'Export - Cost of Sales', N'ወደ ውጭ መላክ - የሽያጭ ዋጋ', N'1110', N'CostOfSales'),
(1120,11, N'Export - SGA', N'ወደ ውጭ መላክ - SGA', N'1120', N'SellingGeneralAndAdministration'),
(116,11, N'Export - Production', N'ወደ ውጭ መላክ - ምርት', N'116', N'Abstract'),
(1161,116, N'Export Factory - Site 1', N'የውጭ ንግድ ፋብሪካ - ጣቢያ 1', N'1161', N'ProductionExpenseControl'),
(1162,116, N'Export Factory - Site 2', N'የውጭ ንግድ ፋብሪካ - የጣቢያ 2', N'1162', N'ProductionExpenseControl'),
(12,1, N'Import', N'አስገባ', N'12', N'Abstract'),
(1210,12, N'Import - Cost of sales', N'አስመጣ - የሽያጭ ዋጋ', N'1210', N'CostOfSales'),
(1220,12, N'Import - SGA', N'አስመጣ - SGA', N'1220', N'SellingGeneralAndAdministration'),
(124,12, N'Import Shipments', N'አስመጣ - በሽግግር ላይ ', N'124', N'Abstract'),
(124001,124, N'Import - LC #1', N'አስመጣ - LC1', N'124001', N'TransitExpenseControl'),
(124002,124, N'Import - LC #2', N'አስመጣ - LC2', N'124002', N'TransitExpenseControl'),
(124003,124, N'Import - LC #3', N'አስመጣ - LC3', N'124003', N'TransitExpenseControl'),
(13,1, N'Agro Processing', N'አግሮ በመስራት ላይ', N'13', N'Abstract'),
(1310,13, N'Agro Processing - Cost of Sales', N'አግሮ ማቀነባበር - የሽያጭ ዋጋ', N'1310', N'CostOfSales'),
(1320,13, N'Agro Processing - SGA', N'አግሮ ማቀነባበር - SGA', N'1320', N'SellingGeneralAndAdministration'),
(1360,13, N'Agro Processing - Production', N'አግሮ ማቀነባበር - ምርት', N'1360', N'ProductionExpenseControl'),
(14,1, N'Manufacturing', N'ማኑፋክቸሪንግ', N'14', N'Abstract'),
(1410,14, N'Manufacturing - Cost of sales', N'ማኑፋክቸሪንግ - የሽያጭ ዋጋ', N'1410', N'CostOfSales'),
(1420,14, N'Manufacturing - SGA', N'ማኑፋክቸሪንግ - SGA', N'1420', N'SellingGeneralAndAdministration'),
(144,14, N'Manufacturing - in transit', N'በማምረቻ - በሽግግር ላይ', N'144', N'Abstract'),
(144001,144, N'Manufacturing - LC #1', N'ማኑፋክቸሪንግ - LC #1', N'144001', N'TransitExpenseControl'),
(144002,144, N'Manufacturing - LC #2', N'ማኑፋክቸሪንግ - LC #2', N'144002', N'TransitExpenseControl'),
(144003,144, N'Manufacturing - LC #3', N'ማኑፋክቸሪንግ - LC #3', N'144003', N'TransitExpenseControl'),
(146,14, N'Manufacturing - Production', N'ማኑፋክቸሪንግ - ምርት', N'146', N'Abstract'),
(146001,146, N'Manufacturing - JO #1', N'ማኑፋክቸሪንግ - JO #1', N'146001', N'ProductionExpenseControl'),
(146002,146, N'Manufacturing - JO #2', N'ማኑፋክቸሪንግ - JO #2', N'146002', N'ProductionExpenseControl'),
(146003,146, N'Manufacturing - JO #3', N'ማኑፋክቸሪንግ - JO #3', N'146003', N'ProductionExpenseControl'),
(15,1, N'Local Trade', N'የአገር ውስጥ ንግድ', N'15', N'Abstract'),
(1510,15, N'Local Trade - Cost of Sales', N'የአገር ውስጥ ንግድ - የሽያጭ ዋጋ', N'1510', N'CostOfSales'),
(1520,15, N'Local Trade - SGA', N'የአገር ውስጥ ንግድ - SGA', N'1520', N'SellingGeneralAndAdministration'),
(156,15, N'Local Trade - Production', N'የአገር ውስጥ ንግድ - ፕሮዳክሽን', N'156', N'Abstract'),
(1561,156, N'Local Trade Factory - Site 1', N'የአገር ውስጥ ንግድ ፋብሪካ - ጣቢያ 1', N'1561', N'ProductionExpenseControl'),
(1562,156, N'Local Trade Factory - Site 2', N'አካባቢያዊ የንግድ ፋብሪካ - የጣቢያ 2', N'1562', N'ProductionExpenseControl'),
(2,NULL, N'Real Estate Centers', N'ሪል እስቴት ማዕከል', N'2', N'Segment'),
(2000,2, N'Real Estate', N'ሪል እስቴት ማዕከል', N'2000', N'Parent'),
(21,2, N'Soreti Mall', N'ሶሬቲ የገቢያ አዳራሽ', N'21', N'Abstract'),
(2110,21, N'Soreti Mall - Cost of sales', N'ሶሬቲ የገቢያ አዳራሽ - የሽያጭ ዋጋ', N'2110', N'CostOfSales'),
(2120,21, N'Soreti Mall - SGA', N'ሶሬቲ የገቢያ አዳራሽ - SGA', N'2120', N'SellingGeneralAndAdministration'),
(22,2, N'AA Building', N'AA ህንፃ', N'22', N'Abstract'),
(2210,22, N'AA Building - Cost of sales', N'AA ህንጻ - የሽያጭ ዋጋ', N'2210', N'CostOfSales'),
(2220,22, N'AA Building - SGA', N'AA ሕንፃ - ስርጭት', N'2220', N'SellingGeneralAndAdministration'),
(2250,22, N'AA Building - Under Construction', N'AA ህንጻ - Under Construction', N'2250', N'ConstructionExpenseControl');

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Declarations
DECLARE @106C_TradingHO INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Trading - HO');
DECLARE @106C_General INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'General');
DECLARE @106C_Management INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Management');
DECLARE @106C_Finance INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Finance');
DECLARE @106C_IT INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'IT');
DECLARE @106C_SoretiAdminSharedExpenses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Soreti Admin - Shared Expenses');
DECLARE @106C_GrainsFactoriesSite1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Grains Factories - Site 1');
DECLARE @106C_GrainsFactoriesSite2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Grains Factories - Site 2');
DECLARE @106C_ExportCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Export - Cost of Sales');
DECLARE @106C_ExportSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Export - SGA');
DECLARE @106C_ExportFactorySite1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Export Factory - Site 1');
DECLARE @106C_ExportFactorySite2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Export Factory - Site 2');
DECLARE @106C_ImportCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Import - Cost of sales');
DECLARE @106C_ImportSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Import - SGA');
DECLARE @106C_ImportLC#1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Import - LC #1');
DECLARE @106C_ImportLC#2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Import - LC #2');
DECLARE @106C_ImportLC#3 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Import - LC #3');
DECLARE @106C_AgroProcessingCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Agro Processing - Cost of Sales');
DECLARE @106C_AgroProcessingSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Agro Processing - SGA');
DECLARE @106C_AgroProcessingProduction INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Agro Processing - Production');
DECLARE @106C_ManufacturingCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Manufacturing - Cost of sales');
DECLARE @106C_ManufacturingSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Manufacturing - SGA');
DECLARE @106C_ManufacturingLC#1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Manufacturing - LC #1');
DECLARE @106C_ManufacturingLC#2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Manufacturing - LC #2');
DECLARE @106C_ManufacturingLC#3 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Manufacturing - LC #3');
DECLARE @106C_ManufacturingJO#1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Manufacturing - JO #1');
DECLARE @106C_ManufacturingJO#2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Manufacturing - JO #2');
DECLARE @106C_ManufacturingJO#3 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Manufacturing - JO #3');
DECLARE @106C_LocalTradeCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Local Trade - Cost of Sales');
DECLARE @106C_LocalTradeSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Local Trade - SGA');
DECLARE @106C_LocalTradeFactorySite1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Local Trade Factory - Site 1');
DECLARE @106C_LocalTradeFactorySite2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Local Trade Factory - Site 2');
DECLARE @106C_RealEstate INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Real Estate');
DECLARE @106C_SoretiMallCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Soreti Mall - Cost of sales');
DECLARE @106C_SoretiMallSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'Soreti Mall - SGA');
DECLARE @106C_AABuildingCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'AA Building - Cost of sales');
DECLARE @106C_AABuildingSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'AA Building - SGA');
DECLARE @106C_AABuildingUnderConstruction INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] NOT IN (N'Segment',N'Abstract') AND [Name] = N'AA Building - Under Construction');