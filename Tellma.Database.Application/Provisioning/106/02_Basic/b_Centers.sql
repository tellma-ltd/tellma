INSERT INTO @Centers([Index],[ParentIndex], [Name],[Name2],[Code], [CenterType]) VALUES 
(0,NULL, N'Soreti', N'ሶሬቲ', N'', N'Segment'),
(1,0, N'Soreti - Unspecified', N'ሶሬቲ - ያልተገለጸ', N'000', N'Common'),
(2,0, N'Profit Centers', N'ትርፍ ማዕከላት', N'100', N'Abstract'),
(3,2, N'Trading Profit Centers', N'የንግድ ትርፍ ማዕከሎች', N'110', N'Abstract'),
(4,3, N'Exported Grains', N'ወደ ውጭ የተላኩ እህሎች', N'111', N'CostOfSales'),
(5,3, N'Processed Oil ', N'የተቀቀለ ዘይት', N'112', N'CostOfSales'),
(6,3, N'Assembled Minidor', N'የተሰበሰበ Minidor', N'113', N'CostOfSales'),
(7,3, N'Imported Merchandise', N'ከውጭ የመጣው ንግድ', N'114', N'CostOfSales'),
(8,3, N'Local Grains', N'የአከባቢ እህሎች', N'115', N'CostOfSales'),
(9,3, N'Other Trading Centers', N'ሌሎች የንግድ ማዕከላት', N'119', N'CostOfSales'),
(10,2, N'Rental Profit Centers', N'የኪራይ ትርፍ ማዕከላት', N'120', N'Abstract'),
(11,10, N'Soreti Mall', N'ሶሬቲ ሜል', N'121', N'CostOfSales'),
(12,10, N'Addis Ababa Building', N'አዲስ አበባ ብሩንዲ', N'122', N'CostOfSales'),
(13,0, N'SGNA Centers', N'SG&A ማዕከላት', N'200', N'Abstract'),
(14,13, N'Bole HQ - SGNA', N'ቦሌ ኤች ኤች - ኤስ.ጂ.ኤን.', N'201', N'AdministrativeExpense'),
(15,0, N'Production Centers', N'የምርት ማዕከላት', N'300', N'Abstract'),
(16,13, N'Adama Factory', N'አዳማ ፋብሪካ', N'301', N'ProductionExtension'),
(17,13, N'Minidor Factory', N'አነስተኛ ፋብሪካ', N'302', N'ProductionExtension');





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
DECLARE @106C_SoretiUnspecified INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti - Unspecified');
DECLARE @106C_ProfitCenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Profit Centers');
DECLARE @106C_TradingProfitCenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Trading Profit Centers');
DECLARE @106C_ExportedGrains INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Exported Grains');
DECLARE @106C_ProcessedOil INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Processed Oil ');
DECLARE @106C_AssembledMinidor INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Assembled Minidor');
DECLARE @106C_ImportedMerchandise INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Imported Merchandise');
DECLARE @106C_LocalGrains INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Local Grains');
DECLARE @106C_OtherTradingCenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Other Trading Centers');
DECLARE @106C_RentalProfitCenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Rental Profit Centers');
DECLARE @106C_SoretiMall INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Soreti Mall');
DECLARE @106C_AddisAbabaBuilding INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Addis Ababa Building');
DECLARE @106C_SGNACenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'SGNA Centers');
DECLARE @106C_BoleHQSGNA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Bole HQ - SGNA');
DECLARE @106C_ProductionCenters INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Production Centers');
DECLARE @106C_AdamaFactory INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Adama Factory');
DECLARE @106C_MinidorFactory INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Minidor Factory');
