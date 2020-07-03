INSERT INTO @Centers([Index],[ParentIndex], [Name],[Name2],[Code], [CenterType]) VALUES
(0,NULL, N'Soreti', N'ሶሬቲ', N'', N'Abstract'),
(1,0, N'Selling, General and Admininstration', N'መሸጥ ፣ አጠቃላይ እና አስተዳደር', N'1', N'Abstract'),
(11,1, N'Soreti -', N'ሶሬቲ -', N'11', N'Abstract'),
(110,11, N'Soreti', N'ሶሬቲ', N'110', N'Parent'),
(12,1, N'Head Office', N'ዋና መስሪያ ቤት', N'12', N'Abstract'),
(120,12, N'General', N'ጠቅላላ', N'120', N'SellingGeneralAndAdministration'),
(121,12, N'Management', N'አስተዳደር', N'121', N'SellingGeneralAndAdministration'),
(122,12, N'Marketing', N'ግብይት', N'122', N'SellingGeneralAndAdministration'),
(123,12, N'Finance', N'ፋይናንስ', N'123', N'SellingGeneralAndAdministration'),
(124,12, N'IT', N'የአይቲ', N'124', N'SellingGeneralAndAdministration'),
(125,12, N'HR', N'HR', N'125', N'SellingGeneralAndAdministration'),
(129,12, N'Head Office - Shared Expenses', N'አስተዳደር - የጋራ ወጪዎች', N'129', N'SharedExpenseControl'),
(13,1, N'Vehicles', N'ተሸከርካሪዎች', N'13', N'Abstract'),
(131,13, N'Vehicle 1', N'ተሽከርካሪ 1', N'131', N'SellingGeneralAndAdministration'),
(132,13, N'Vehicle 2', N'ተሽከርካሪ 2', N'132', N'SellingGeneralAndAdministration'),
(133,13, N'Vehicle 3', N'ተሽከርካሪ 3', N'133', N'SellingGeneralAndAdministration'),
(139,13, N'Vehicles - Shared Expenses', N'ተሽከርካሪዎች - የጋራ ወጪዎች', N'139', N'SharedExpenseControl'),
(3,0, N'Shared Production Centers', N'የምርት ማዕከላት', N'3', N'Abstract'),
(31,3, N'Adama - Site 1', N'አዳማ - ጣቢያ 1', N'31', N'Abstract'),
(311,31, N'Site 1 - Cleaning and Milling', N'ጣቢያ 1 - ጽዳት እና መፍጨት', N'311', N'SharedExpenseControl'),
(32,3, N'Adama - Site 2', N'አዳማ - ጣቢያ 2', N'32', N'Abstract'),
(321,32, N'Site 2 - Local and Export Grain Cleaning', N'ጣቢያ 2 - አካባቢያዊ እና ላክ የቅንጣት ጽዳት', N'321', N'SharedExpenseControl'),
(5,0, N'Trading Centers', N'ትሬዲንግ ማዕከል', N'5', N'Abstract'),
(51,5, N'Export', N'ወደ ውጭ የተላኩ እህሎች', N'51', N'Abstract'),
(511,51, N'Export - Cost of Sales', N'ወደ ውጭ መላክ - የሽያጭ ዋጋ', N'511', N'CostOfSales'),
(512,51, N'Export - SGA', N'ወደ ውጭ መላክ - SGA', N'512', N'SellingGeneralAndAdministration'),
(513,51, N'Site 1 - Export Grain Cleaning', N'ጣቢያ 1 - ላክ የቅንጣት ጽዳት', N'513', N'ProductionExpenseControl'),
(514,51, N'Site 2 - Export Grain Cleaning', N'ጣቢያ 2 - ላክ የቅንጣት ጽዳት', N'514', N'ProductionExpenseControl'),
(519,51, N'Export Shipments', N'ወደ ውጭ ይላኩ መርከቦች', N'519', N'Abstract'),
(52,5, N'Import', N'አስገባ', N'52', N'Abstract'),
(521,52, N'Import - Cost of sales', N'አስመጣ - የሽያጭ ዋጋ', N'521', N'CostOfSales'),
(522,52, N'Import - SGA', N'አስመጣ - SGA', N'522', N'SellingGeneralAndAdministration'),
(523,52, N'Import Shipments', N'አስመጣ - በሽግግር ላይ ', N'523', N'Abstract'),
(53,5, N'Agro Processing', N'አግሮ በመስራት ላይ', N'53', N'Abstract'),
(531,53, N'Agro Processing - Cost of Sales', N'አግሮ ማቀነባበር - የሽያጭ ዋጋ', N'531', N'CostOfSales'),
(532,53, N'Agro Processing - SGA', N'አግሮ ማቀነባበር - SGA', N'532', N'SellingGeneralAndAdministration'),
(533,53, N'Oil Milling Line', N'ዘይት ሚሊ', N'533', N'ProductionExpenseControl'),
(54,5, N'Manufacturing', N'ማኑፋክቸሪንግ', N'54', N'Abstract'),
(541,54, N'Manufacturing - Cost of sales', N'ማኑፋክቸሪንግ - የሽያጭ ዋጋ', N'541', N'CostOfSales'),
(542,54, N'Manufacturing - SGA', N'ማኑፋክቸሪንግ - SGA', N'542', N'SellingGeneralAndAdministration'),
(543,54, N'Minidor Line', N'አነስተኛ መስመር', N'543', N'ProductionExpenseControl'),
(544,54, N'Minidor Import Shipments', N'Minidor አስመጣ አላላኮችን', N'544', N'Abstract'),
(55,5, N'Local Trade', N'የአገር ውስጥ ንግድ', N'55', N'Abstract'),
(551,55, N'Local Trade - Cost of Sales', N'የአገር ውስጥ ንግድ - የሽያጭ ዋጋ', N'551', N'CostOfSales'),
(552,55, N'Local Trade - SGA', N'የአገር ውስጥ ንግድ - SGA', N'552', N'SellingGeneralAndAdministration'),
(553,51, N'Site 1 - Local Grain Cleaning', N'ጣቢያ 1 - አካባቢያዊ የቅንጣት ጽዳት', N'553', N'ProductionExpenseControl'),
(554,51, N'Site 2 - Local Grain Cleaning', N'ጣቢያ 2 - አካባቢያዊ የቅንጣት ጽዳት', N'554', N'ProductionExpenseControl'),
(6,0, N'Real Estate', N'ሪል እስቴት ማዕከል', N'6', N'Abstract'),
(61,6, N'Soreti Mall', N'ሶሬቲ የገቢያ አዳራሽ', N'61', N'Abstract'),
(611,61, N'Soreti Mall - Cost of sales', N'ሶሬቲ የገቢያ አዳራሽ - የሽያጭ ዋጋ', N'611', N'CostOfSales'),
(612,61, N'Soreti Mall - SGA', N'ሶሬቲ የገቢያ አዳራሽ - SGA', N'612', N'SellingGeneralAndAdministration'),
(613,62, N'Soreti Mall Construction', N'Soሶሬቲ የገቢያ አዳራሽ - ኮንስትራክሽን', N'613', N'ConstructionExpenseControl'),
(62,6, N'AA Building', N'AA ህንፃ', N'62', N'Abstract'),
(621,62, N'AA Building - Cost of sales', N'AA ህንጻ - የሽያጭ ዋጋ', N'621', N'CostOfSales'),
(622,62, N'AA Building - SGA', N'AA ህንፃ - ስርጭት', N'622', N'SellingGeneralAndAdministration'),
(623,62, N'AA Building - Construction', N'AA ህንፃ - ኮንስትራክሽን', N'623', N'ConstructionExpenseControl');

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