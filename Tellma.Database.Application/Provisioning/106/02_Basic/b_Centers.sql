INSERT INTO @Centers([Index],[ParentIndex], [Name],[Name2],[Code], [CenterType]) VALUES
(0,NULL, N'Soreti', N'ሶሬቲ', N'0', N'Abstract'),
(1,0, N'Head Office', N'ዋና መስሪያ ቤት', N'1', N'BusinessUnit'),
(11,1, N'Selling, General and Admininstration', N'መሸጥ ፣ አጠቃላይ እና አስተዳደር', N'11', N'Abstract'),
(110,11, N'Head Office - Shared Expenses', N'አስተዳደር - የጋራ ወጪዎች', N'110', N'SharedExpenseControl'),
(111,11, N'Management', N'አስተዳደር', N'111', N'SellingGeneralAndAdministration'),
(112,11, N'Marketing', N'ግብይት', N'112', N'SellingGeneralAndAdministration'),
(113,11, N'Finance', N'ፋይናንስ', N'113', N'SellingGeneralAndAdministration'),
(114,11, N'IT', N'የአይቲ', N'114', N'SellingGeneralAndAdministration'),
(115,11, N'HR', N'HR', N'115', N'SellingGeneralAndAdministration'),
(119,11, N'General', N'ጠቅላላ', N'119', N'SellingGeneralAndAdministration'),
(12,1, N'Vehicles', N'ተሸከርካሪዎች', N'12', N'Abstract'),
(121,12, N'Vehicle 1', N'ተሽከርካሪ 1', N'121', N'SellingGeneralAndAdministration'),
(122,12, N'Vehicle 2', N'ተሽከርካሪ 2', N'122', N'SellingGeneralAndAdministration'),
(123,12, N'Vehicle 3', N'ተሽከርካሪ 3', N'123', N'SellingGeneralAndAdministration'),
(129,12, N'Vehicles - Shared Expenses', N'ተሽከርካሪዎች - የጋራ ወጪዎች', N'129', N'SharedExpenseControl'),
(13,1, N'Shared Production Centers', N'የምርት ማዕከላት', N'13', N'Abstract'),
(131,13, N'Site 1 - Cleaning and Milling', N'ጣቢያ 1 - ጽዳት እና መፍጨት', N'131', N'SharedExpenseControl'),
(132,13, N'Site 2 - Local and Export Grain Cleaning', N'ጣቢያ 2 - አካባቢያዊ እና ላክ የቅንጣት ጽዳት', N'132', N'SharedExpenseControl'),
(2,NULL, N'Export', N'ወደ ውጪ ላክ', N'2', N'BusinessUnit'),
(20,2, N'Export - Cost of sales', N'ላክ - የሽያጭ ዋጋ', N'20', N'Abstract'),
(200,20, N'Export - Cost of sales -', N'ወደ ውጪ ላክ - የሽያጭ ዋጋ -', N'200', N'CostOfSales'),
(21,2, N'Export - SGA', N'ወደ ውጭ መላክ - SGA', N'21', N'Abstract'),
(210,21, N'Export - SGA -', N'ወደ ውጭ መላክ - SGA -', N'210', N'SellingGeneralAndAdministration'),
(23,2, N'Export - Production Centers', N'ላክ - ፕሮዳክሽን ማዕከል', N'23', N'Abstract'),
(231,23, N'Site 1 - Export Grain Cleaning', N'ጣቢያ 1 - ላክ የቅንጣት ጽዳት', N'231', N'ProductionExpenseControl'),
(232,23, N'Site 2 - Export Grain Cleaning', N'ጣቢያ 2 - ላክ የቅንጣት ጽዳት', N'232', N'ProductionExpenseControl'),
(25,2, N'Export Shipments', N'ወደ ውጭ ይላኩ መርከቦች', N'25', N'Abstract'),
(251,25, N'Export Permit 1', N'የውጭ ንግድ ፈቃድ 1', N'251', N'TransitExpenseControl'),
(252,25, N'Export Permit 2', N'የውጭ ንግድ ፈቃድ 2', N'252', N'TransitExpenseControl'),
(3,NULL, N'Import', N'አስገባ', N'3', N'BusinessUnit'),
(30,3, N'Import - Cost of sales', N'አስመጣ - የሽያጭ ዋጋ', N'30', N'CostOfSales'),
(300,30, N'Import - Cost of sales -', N'አስመጣ - የሽያጭ ዋጋ -', N'300', N'CostOfSales'),
(31,3, N'Import - SGA', N'አስመጣ - SGA', N'31', N'SellingGeneralAndAdministration'),
(310,31, N'Import - SGA -', N'አስመጣ - SGA -', N'310', N'SellingGeneralAndAdministration'),
(32,3, N'Import Shipments', N'አስመጣ - በሽግግር ላይ ', N'32', N'Abstract'),
(321,32, N'Import LC 1', N'አስመጣ LC 1', N'321', N'TransitExpenseControl'),
(322,32, N'Import LC 2', N'አስመጣ LC 2', N'322', N'TransitExpenseControl'),
(4,NULL, N'Agro Processing', N'አግሮ በመስራት ላይ', N'4', N'BusinessUnit'),
(40,4, N'Agro Processing - Cost of Sales', N'አግሮ ማቀነባበር - የሽያጭ ዋጋ', N'40', N'Abstract'),
(400,40, N'Agro Processing - Cost of Sales -', N'አግሮ ማቀነባበር - የሽያጭ ዋጋ -', N'400', N'CostOfSales'),
(41,4, N'Agro Processing - SGA', N'አግሮ ማቀነባበር - SGA', N'41', N'Abstract'),
(410,41, N'Agro Processing - SGA -', N'አግሮ ማቀነባበር - SGA -', N'410', N'SellingGeneralAndAdministration'),
(43,4, N'Agro - Production Centers', N'አግሮ - ፕሮዳክሽን ማዕከል', N'43', N'Abstract'),
(431,43, N'Oil Milling Line', N'ዘይት ሚሊ', N'431', N'ProductionExpenseControl'),
(5,NULL, N'Manufacturing', N'ማኑፋክቸሪንግ', N'5', N'BusinessUnit'),
(50,5, N'Manufacturing - Cost of sales', N'ማኑፋክቸሪንግ - የሽያጭ ዋጋ', N'50', N'Abstract'),
(500,50, N'Manufacturing - Cost of sales -', N'ማኑፋክቸሪንግ - የሽያጭ ዋጋ -', N'500', N'CostOfSales'),
(51,5, N'Manufacturing - SGA', N'ማኑፋክቸሪንግ - SGA', N'51', N'Abstract'),
(510,51, N'Manufacturing - SGA -', N'ማኑፋክቸሪንግ - SGA -', N'510', N'SellingGeneralAndAdministration'),
(52,5, N'Minidor Import Shipments', N'Minidor አስመጣ አላላኮችን', N'52', N'Abstract'),
(521,52, N'Minidor Import LC 1', N'Minidor አስመጣ LC 1', N'521', N'TransitExpenseControl'),
(522,52, N'Minidor Import LC 2', N'Minidor አስመጣ LC 2', N'522', N'TransitExpenseControl'),
(53,5, N'Manufacturing - Production Centers', N'ማኑፋክቸሪንግ - ፕሮዳክሽን ማዕከል', N'53', N'Abstract'),
(531,53, N'Minidor Line', N'አነስተኛ መስመር', N'531', N'ProductionExpenseControl'),
(6,NULL, N'Local Trade', N'የአገር ውስጥ ንግድ', N'6', N'BusinessUnit'),
(60,6, N'Local Trade - Cost of Sales', N'የአገር ውስጥ ንግድ - የሽያጭ ዋጋ', N'60', N'Abstract'),
(600,60, N'Local Trade - Cost of Sales -', N'የአገር ውስጥ ንግድ - የሽያጭ ዋጋ -', N'600', N'CostOfSales'),
(61,6, N'Local Trade - SGA', N'የአገር ውስጥ ንግድ - SGA', N'61', N'Abstract'),
(610,61, N'Local Trade - SGA -', N'የአገር ውስጥ ንግድ - SGA -', N'610', N'SellingGeneralAndAdministration'),
(63,6, N'Local Trade - Production Centers', N'አካባቢያዊ ንግድ - ፕሮዳክሽን ማዕከል', N'63', N'Abstract'),
(631,63, N'Site 1 - Local Grain Cleaning', N'ጣቢያ 1 - አካባቢያዊ የቅንጣት ጽዳት', N'631', N'ProductionExpenseControl'),
(632,63, N'Site 2 - Local Grain Cleaning', N'ጣቢያ 2 - አካባቢያዊ የቅንጣት ጽዳት', N'632', N'ProductionExpenseControl'),
(7,NULL, N'Real Estate', N'መጠነሰፊ የቤት ግንባታ', N'7', N'BusinessUnit'),
(70,7, N'Real Estate - Direct Expenses', N'የማይንቀሳቀስ ንብረት - ቀጥተኛ ወጪዎች', N'70', N'Abstract'),
(700,70, N'Real Estate - Direct Expenses -', N'የማይንቀሳቀስ ንብረት - ቀጥተኛ ወጪዎች -', N'700', N'CostOfSales'),
(71,7, N'Real Estate - SGA', N'የማይንቀሳቀስ ንብረት - SGA', N'71', N'Abstract'),
(711,71, N'Soreti Mall - SGA', N'ሶሬቲ የገቢያ አዳራሽ - SGA', N'711', N'SellingGeneralAndAdministration'),
(712,71, N'AA Building - SGA', N'AA ህንፃ - ስርጭት', N'712', N'SellingGeneralAndAdministration'),
(74,7, N'Real Estate - Construction Projects', N'የማይንቀሳቀስ ንብረት - የግንባታ ፕሮጀክቶች', N'74', N'Abstract'),
(741,74, N'Soreti Mall Construction Project', N'ሶሬቲ የገቢያ አዳራሽ - የግንባታ ፕሮጀክቶች', N'741', N'ConstructionExpenseControl'),
(742,74, N'AA Building Construction Project', N'AA ሕንፃ ግንባታ ፕሮጀክት', N'742', N'ConstructionExpenseControl');

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Declarations
DECLARE @106C_Soreti INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Soreti');
DECLARE @106C_General INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'General');
DECLARE @106C_Management INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Management');
DECLARE @106C_Marketing INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Marketing');
DECLARE @106C_Finance INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Finance');
DECLARE @106C_IT INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'IT');
DECLARE @106C_HR INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'HR');
DECLARE @106C_HeadOfficeSharedExpenses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Head Office - Shared Expenses');
DECLARE @106C_Vehicle1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Vehicle 1');
DECLARE @106C_Vehicle2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Vehicle 2');
DECLARE @106C_Vehicle3 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Vehicle 3');
DECLARE @106C_VehiclesSharedExpenses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Vehicles - Shared Expenses');
DECLARE @106C_Site1CleaningandMilling INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Site 1 - Cleaning and Milling');
DECLARE @106C_Site2LocalandExportGrainCleaning INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Site 2 - Local and Export Grain Cleaning');
DECLARE @106C_ExportCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Export - Cost of Sales');
DECLARE @106C_ExportSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Export - SGA');
DECLARE @106C_Site1ExportGrainCleaning INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Site 1 - Export Grain Cleaning');
DECLARE @106C_Site2ExportGrainCleaning INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Site 2 - Export Grain Cleaning');
DECLARE @106C_ImportCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Import - Cost of sales');
DECLARE @106C_ImportSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Import - SGA');
DECLARE @106C_AgroProcessingCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Agro Processing - Cost of Sales');
DECLARE @106C_AgroProcessingSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Agro Processing - SGA');
DECLARE @106C_OilMillingLine INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Oil Milling Line');
DECLARE @106C_ManufacturingCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Manufacturing - Cost of sales');
DECLARE @106C_ManufacturingSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Manufacturing - SGA');
DECLARE @106C_MinidorLine INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Minidor Line');
DECLARE @106C_LocalTradeCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Local Trade - Cost of Sales');
DECLARE @106C_LocalTradeSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Local Trade - SGA');
DECLARE @106C_Site1LocalGrainCleaning INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Site 1 - Local Grain Cleaning');
DECLARE @106C_Site2LocalGrainCleaning INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Site 2 - Local Grain Cleaning');
DECLARE @106C_SoretiMallCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Soreti Mall - Cost of sales');
DECLARE @106C_SoretiMallSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Soreti Mall - SGA');
DECLARE @106C_SoretiMallConstruction INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Soreti Mall Construction');
DECLARE @106C_AABuildingCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'AA Building - Cost of sales');
DECLARE @106C_AABuildingSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'AA Building - SGA');
DECLARE @106C_AABuildingConstruction INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'AA Building - Construction');