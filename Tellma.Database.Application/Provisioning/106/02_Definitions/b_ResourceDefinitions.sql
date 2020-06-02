
	INSERT INTO @ResourceDefinitions([Index], [Code], [TitleSingular], [TitleSingular2], [TitleSingular3], [TitlePlural], [TitlePlural2], [TitlePlural3], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
	(0, N'office-equipment', N'Office equipment', N'የቢሮ መሣሪያዎች', N'Office equipment', N'Office equipment', N'የቢሮ መሣሪያዎች', N'Office equipment', N'chair-office', N'Assets',60),
	(1, N'machinery', N'Machinery', N'ማሽኖች', N'Machinery', N'Machineries', N'ማሽኖች', N'Machineries', N'cogs', N'Assets',70),
	(2, N'vehicles', N'Vehicles', N'ተሽከርካሪዎች', N'Vehicles', N'Vehicles', N'ተሽከርካሪዎች', N'Vehicles', N'cars', N'Assets',40),
	(3, N'buildings', N'Building', N'ህንፃዎች', N'Building', N'Buildings', N'ህንፃዎች', N'Buildings', N'building', N'Assets',50),
	(4, N'investment-properties', N'Investment property', N'የኢንmentስትሜንት ንብረቶች', N'Investment property', N'Investment properties', N'የኢንmentስትሜንት ንብረቶች', N'Investment properties', N'city', N'Assets',80),
	(5, N'raw-grains', N'Raw grain', N'ጥሬ እህሎች', N'Raw grain', N'Raw grains', N'ጥሬ እህሎች', N'Raw grains', N'wheat', N'Purchasing',90),
	(6, N'finished-grains', N'Cleaned grain', N'የተጣራ እህል', N'Cleaned grain', N'Cleaned grains', N'የተጣራ እህል', N'Cleaned grains', N'wheat', N'Production',100),
	(7, N'byproducts-grains', N'Reject grain', N'እህልን ይከልክሉ', N'Reject grain', N'Reject grains', N'እህልን ይከልክሉ', N'Reject grains', N'wheat', N'Production',60),
	(8, N'raw-vehicles', N'Vehicles component', N'የተሽከርካሪዎች ክፍሎች', N'Vehicles component', N'Vehicles components', N'የተሽከርካሪዎች ክፍሎች', N'Vehicles components', N'tire', N'Purchasing',120),
	(9, N'finished-vehicles', N'Assembled vehicle', N'የተሰበሰቡ ተሽከርካሪዎች', N'Assembled vehicle', N'Assembled vehicles', N'የተሰበሰቡ ተሽከርካሪዎች', N'Assembled vehicles', N'car-side', N'Production',120),
	(10, N'raw-oils', N'Raw materials (Oil Milling)', N'ጥሬ እቃዎች (ዘይት ቁፋሮ)', N'Raw materials (Oil Milling)', N'Raw materials (Oil Milling)', N'ጥሬ እቃዎች (ዘይት ቁፋሮ)', N'Raw materials (Oil Milling)', N'file-export', N'Purchasing',121),
	(11, N'finished-oils', N'Processed Oil (Milling)', N'የተቀቀለ ዘይት (ወፍጮ)', N'Processed Oil (Milling)', N'Processed Oil (Milling)', N'የተቀቀለ ዘይት (ወፍጮ)', N'Processed Oil (Milling)', N'tint', N'Production',122),
	(12, N'byproducts-oils', N'Oil byproduct', N'የዘይት ፍሬ', N'Oil byproduct', N'Oil byproducts', N'ዘይት ያመርታል', N'Oil byproducts', N'tint-slash', N'Production',123),
	(13, N'work-in-progress', N'Work in progress', N'ገና በሂደት ላይ ያለ ስራ', N'Work in progress', N'Work in progress', N'ገና በሂደት ላይ ያለ ስራ', N'Work in progress', N'spinner', N'Production',124),
	(14, N'medicines', N'Medicine', N'መድሃኒት', N'Medicine', N'Medicines', N'መድሃኒቶች', N'Medicines', N'pills', N'Purchasing',125),
	(15, N'construction-materials', N'Construction material', N'የግንባታ ቁሳቁሶች', N'Construction material', N'Construction materials', N'የግንባታ ቁሳቁሶች', N'Construction materials', N'building', N'Purchasing',126);
EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

DECLARE @106office_equipmentRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'office-equipment');
DECLARE @106machineryRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'machinery');
DECLARE @106vehiclesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'vehicles');
DECLARE @106buildingsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'buildings');
DECLARE @106investment_propertiesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'investment-properties');
DECLARE @106raw_grainsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'raw-grains');
DECLARE @106finished_grainsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'finished-grains');
DECLARE @106byproducts_grainsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'byproducts-grains');
DECLARE @106raw_vehiclesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'raw-vehicles');
DECLARE @106finished_vehiclesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'finished-vehicles');
DECLARE @106raw_oilsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'raw-oils');
DECLARE @106finished_oilsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'finished-oils');
DECLARE @106byproducts_oilsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'byproducts-oils');
DECLARE @106work_in_progressRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'work-in-progress');
DECLARE @106medicinesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'medicines');
DECLARE @106construction_materialsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'construction-materials');


IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Resource Definitions Standard: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;		