INSERT INTO @Centers([Index],[ParentIndex], [Name],[Name2],[Code], [CenterType]) VALUES 
(0,NULL, N'Soreti - Unspecified', N'ሶሬቲ - ያልተገለጸ', N'000', N'Common'),
(1,NULL, N'SGNA Centers', N'SG&A ማዕከላት', N'100', N'Abstract'),
(2,1, N'Bole HQ - SGNA', N'ቦሌ ኤች ኤች - ኤስ.ጂ.ኤን.', N'101', N'AdministrativeExpense'),
(3,NULL, N'Production Centers', N'የምርት ማዕከላት', N'200', N'Abstract'),
(4,3, N'Site 1 - Oil - Grains', N'ጣቢያ 1 - ዘይት - እህሎች', N'201', N'ProductionExtension'),
(5,3, N'Site 2 - Grains', N'ጣቢያ 2 - እህሎች', N'202', N'ProductionExtension'),
(6,3, N'Minidor Factory', N'አነስተኛ ፋብሪካ', N'203', N'ProductionExtension'),
(7,NULL, N'Profit Centers', N'ትርፍ ማዕከላት', N'300', N'Abstract'),
(8,7, N'Trading Profit Centers', N'የንግድ ትርፍ ማዕከሎች', N'310', N'Abstract'),
(9,8, N'Exported Grains', N'ወደ ውጭ የተላኩ እህሎች', N'311', N'CostOfSales'),
(10,8, N'Processed Oil ', N'የተቀቀለ ዘይት', N'312', N'CostOfSales'),
(11,8, N'Assembled Minidor', N'የተሰበሰበ Minidor', N'313', N'CostOfSales'),
(12,8, N'Imported Merchandise', N'ከውጭ የመጣው ንግድ', N'314', N'CostOfSales'),
(13,8, N'Other Trading Centers', N'ሌሎች የንግድ ማዕከላት', N'319', N'CostOfSales'),
(14,7, N'Rental Profit Centers', N'የኪራይ ትርፍ ማዕከላት', N'400', N'Abstract'),
(15,14, N'Soreti Mall', N'ሶሬቲ ሜል', N'401', N'CostOfSales'),
(16,14, N'Addis Ababa Building', N'አዲስ አበባ ብሩንዲ', N'402', N'CostOfSales');

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Declarations
DECLARE @106C_SoretiUnspecified INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti - Unspecified');
DECLARE @106C_SGNACenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'SGNA Centers');
DECLARE @106C_BoleHQSGNA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Bole HQ - SGNA');
DECLARE @106C_ProductionCenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Production Centers');
DECLARE @106C_Site1OilGrains INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Site 1 - Oil - Grains');
DECLARE @106C_Site2Grains INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Site 2 - Grains');
DECLARE @106C_MinidorFactory INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Minidor Factory');
DECLARE @106C_ProfitCenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Profit Centers');
DECLARE @106C_TradingProfitCenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Trading Profit Centers');
DECLARE @106C_ExportedGrains INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Exported Grains');
DECLARE @106C_ProcessedOil INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Processed Oil ');
DECLARE @106C_AssembledMinidor INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Assembled Minidor');
DECLARE @106C_ImportedMerchandise INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Imported Merchandise');
DECLARE @106C_OtherTradingCenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Other Trading Centers');
DECLARE @106C_RentalProfitCenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Rental Profit Centers');
DECLARE @106C_SoretiMall INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti Mall');
DECLARE @106C_AddisAbabaBuilding INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Addis Ababa Building');