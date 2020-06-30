INSERT INTO @Centers([Index],[ParentIndex], [Name],[Name2],[Code], [CenterType]) VALUES
(0,NULL, N'Soreti', N'ሶሬቲ', N'', N'Abstract'),
(1,0, N'Selling, General and Admininstration', N'መሸጥ ፣ አጠቃላይ እና አስተዳደር', N'1', N'Abstract'),
(11,1, N'Soreti -', N'ሶሬቲ -', N'11', N'Abstract'),
(110,11, N'Soreti', N'ሶሬቲ', N'110', N'Parent'),
(12,1, N'Head Ofice', N'አስተዳደር', N'12', N'Abstract'),
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
(2,0, N'Transit Centers', N'የመጓጓዣ ማዕከላት', N'2', N'Abstract'),
(21,2, N'Import Shipments', N'አስመጣ - በሽግግር ላይ ', N'21', N'Abstract'),
(211,21, N'Import - LC #1', N'አስመጣ - LC1', N'211', N'TransitExpenseControl'),
(212,21, N'Import - LC #2', N'አስመጣ - LC2', N'212', N'TransitExpenseControl'),
(213,21, N'Import - LC #3', N'አስመጣ - LC3', N'213', N'TransitExpenseControl'),
(22,2, N'Export Shipments', N'ወደ ውጭ ይላኩ መርከቦች', N'22', N'Abstract'),
(221,22, N'Export - Permit #1', N'ወደ ውጭ መላክ - ፈቃድ # 1', N'221', N'TransitExpenseControl'),
(222,22, N'Export - Permit #2', N'ወደ ውጭ መላክ - ፈቃድ # 2', N'222', N'TransitExpenseControl'),
(223,22, N'Export - Permit #3', N'ወደ ውጭ መላክ - ፈቃድ # 3', N'223', N'TransitExpenseControl'),
(3,0, N'Production Centers', N'የምርት ማዕከላት', N'3', N'Abstract'),
(31,3, N'Adama - Site 1', N'አዳማ - ጣቢያ 1', N'31', N'Abstract'),
(311,31, N'Site 1 - Grain Cleaning', N'ጣቢያ 1 - የእህል ማፅጃ', N'311', N'ProductionExpenseControl'),
(312,31, N'Site 1 - Oil Milling', N'ጣቢያ 1 - ዘይት ሚሊ', N'312', N'ProductionExpenseControl'),
(319,31, N'Site 1 - Shared Expenses', N'ጣቢያ 1 - የተጋሩ ወጪዎች', N'319', N'SharedExpenseControl'),
(32,3, N'Adama - Site 2', N'አዳማ - ጣቢያ 2', N'32', N'Abstract'),
(321,32, N'Site 2 - Grain Cleaning', N'ጣቢያ 2 - የእህል ፋብሪካ', N'321', N'ProductionExpenseControl'),
(33,3, N'Modjo Factory', N'ሞጆ ፋብሪካ', N'33', N'Abstract'),
(331,33, N'Minidor Line', N'አነስተኛ መስመር', N'331', N'ProductionExpenseControl'),
(4,0, N'Construction Centers', N'የግንባታ ማዕከላት', N'4', N'Abstract'),
(41,4, N'AA Projects', N'የ AA ፕሮጄክቶች', N'41', N'Abstract'),
(411,41, N'AA Building', N'ኤአ ህንፃ', N'411', N'ProductionExpenseControl'),
(42,4, N'Adama Projects', N'የአዳማ ፕሮጄክቶች', N'42', N'Abstract'),
(421,42, N'Project 1', N'ፕሮጀክት 1', N'421', N'ProductionExpenseControl'),
(422,42, N'Project 2', N'ፕሮጀክት 2', N'422', N'ProductionExpenseControl'),
(5,0, N'Profit Centers', N'ትርፍ ማዕከላት', N'5', N'Abstract'),
(51,5, N'Export', N'ወደ ውጭ የተላኩ እህሎች', N'51', N'Abstract'),
(511,51, N'Export - Cost of Sales', N'ወደ ውጭ መላክ - የሽያጭ ዋጋ', N'511', N'CostOfSales'),
(512,51, N'Export - SGA', N'ወደ ውጭ መላክ - SGA', N'512', N'SellingGeneralAndAdministration'),
(52,5, N'Import', N'አስገባ', N'52', N'Abstract'),
(521,52, N'Import - Cost of sales', N'አስመጣ - የሽያጭ ዋጋ', N'521', N'CostOfSales'),
(522,52, N'Import - SGA', N'አስመጣ - SGA', N'522', N'SellingGeneralAndAdministration'),
(53,5, N'Agro Processing', N'አግሮ በመስራት ላይ', N'53', N'Abstract'),
(531,53, N'Agro Processing - Cost of Sales', N'አግሮ ማቀነባበር - የሽያጭ ዋጋ', N'531', N'CostOfSales'),
(532,53, N'Agro Processing - SGA', N'አግሮ ማቀነባበር - SGA', N'532', N'SellingGeneralAndAdministration'),
(54,5, N'Manufacturing', N'ማኑፋክቸሪንግ', N'54', N'Abstract'),
(541,54, N'Manufacturing - Cost of sales', N'ማኑፋክቸሪንግ - የሽያጭ ዋጋ', N'541', N'CostOfSales'),
(542,54, N'Manufacturing - SGA', N'ማኑፋክቸሪንግ - SGA', N'542', N'SellingGeneralAndAdministration'),
(55,5, N'Local Trade', N'የአገር ውስጥ ንግድ', N'55', N'Abstract'),
(551,55, N'Local Trade - Cost of Sales', N'የአገር ውስጥ ንግድ - የሽያጭ ዋጋ', N'551', N'CostOfSales'),
(552,55, N'Local Trade - SGA', N'የአገር ውስጥ ንግድ - SGA', N'552', N'SellingGeneralAndAdministration'),
(6,0, N'Real Estate', N'ሪል እስቴት ማዕከል', N'6', N'Abstract'),
(61,6, N'Soreti Mall', N'ሶሬቲ የገቢያ አዳራሽ', N'61', N'Abstract'),
(611,61, N'Soreti Mall - Cost of sales', N'ሶሬቲ የገቢያ አዳራሽ - የሽያጭ ዋጋ', N'611', N'CostOfSales'),
(612,61, N'Soreti Mall - SGA', N'ሶሬቲ የገቢያ አዳራሽ - SGA', N'612', N'SellingGeneralAndAdministration'),
(62,6, N'AA Building', N'AA ህንፃ', N'62', N'Abstract'),
(621,62, N'AA Building - Cost of sales', N'AA ህንጻ - የሽያጭ ዋጋ', N'621', N'CostOfSales'),
(622,62, N'AA Building - SGA', N'AA ሕንፃ - ስርጭት', N'622', N'SellingGeneralAndAdministration');
EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Declarations
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
DECLARE @106C_ImportLC1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Import - LC #1');
DECLARE @106C_ImportLC2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Import - LC #2');
DECLARE @106C_ImportLC3 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Import - LC #3');
DECLARE @106C_ExportPermit1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Export - Permit #1');
DECLARE @106C_ExportPermit2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Export - Permit #2');
DECLARE @106C_ExportPermit3 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Export - Permit #3');
DECLARE @106C_Site1GrainCleaning INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Site 1 - Grain Cleaning');
DECLARE @106C_Site1OilMilling INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Site 1 - Oil Milling');
DECLARE @106C_Site1SharedExpenses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Site 1 - Shared Expenses');
DECLARE @106C_Site2GrainCleaning INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Site 2 - Grain Cleaning');
DECLARE @106C_MinidorLine INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Minidor Line');
DECLARE @106C_AABuilding INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'AA Building');
DECLARE @106C_Project1 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Project 1');
DECLARE @106C_Project2 INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Project 2');
DECLARE @106C_ExportCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Export - Cost of Sales');
DECLARE @106C_ExportSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Export - SGA');
DECLARE @106C_ImportCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Import - Cost of sales');
DECLARE @106C_ImportSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Import - SGA');
DECLARE @106C_AgroProcessingCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Agro Processing - Cost of Sales');
DECLARE @106C_AgroProcessingSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Agro Processing - SGA');
DECLARE @106C_ManufacturingCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Manufacturing - Cost of sales');
DECLARE @106C_ManufacturingSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Manufacturing - SGA');
DECLARE @106C_LocalTradeCostofSales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Local Trade - Cost of Sales');
DECLARE @106C_LocalTradeSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Local Trade - SGA');
DECLARE @106C_SoretiMallCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Soreti Mall - Cost of sales');
DECLARE @106C_SoretiMallSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'Soreti Mall - SGA');
DECLARE @106C_AABuildingCostofsales INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'AA Building - Cost of sales');
DECLARE @106C_AABuildingSGA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [CenterType] <> N'Abstract' AND [Name] = N'AA Building - SGA');