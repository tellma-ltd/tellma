	INSERT INTO @Centers([Index], [Name],[Name2],[Code], [CenterType], [ParentIndex], [IsLeaf]) VALUES 
	(0, N'Head Office Segment', N'ዋና መስሪያ ቤት ክፍል', N'101', N'Segment', NULL, 1),
	(1, N'Trading Segment', N'የግብይት ክፍል', N'201', N'Segment', NULL, 1),
	(2, N'Export', N'ወደ ውጭ ይላኩ', N'21', N'Profit', NULL, 0),
	(3, N'Cereals', N'እህል', N'211', N'Profit', 2, 1),
	(4, N'Pulses', N'ጥራጥሬዎች', N'212', N'Profit', 2, 1),
	(5, N'Oilseeds', N'የቅባት እህሎች', N'213', N'Profit', 2, 1),
	(6, N'Import', N'አስመጣ', N'22', N'Profit', NULL, 0),
	(7, N'Spare Parts', N'መለዋወጫ አካላት', N'221', N'Profit', 6, 1),
	(8, N'Medicine', N'መድሃኒት', N'222', N'Profit', 6, 1),
	(9, N'Construction', N'ግንባታ', N'223', N'Profit', 6, 1),
	(10, N'Food Items', N'የምግብ አይነቶች', N'224', N'Profit', 6, 1),
	(11, N'Minidor', N'አነስተኛ', N'231', N'Profit', NULL, 1),
	(12, N'Oil Mills', N'ዘይት ወፍጮዎች', N'241', N'Profit', NULL, 1),
	(13, N'Real Estate Segment', N'የሪል እስቴት ክፍል', N'301', N'Segment', NULL, 1),
	(14, N'Rental', N'ኪራይ', N'31', N'Profit', NULL, 0),
	(15, N'Rental - Adama', N'ኪራይ - አዳማ', N'311', N'Profit', 14, 1),
	(16, N'Rental - AA', N'ኪራይ - ኤኤ', N'312', N'Profit', 14, 1);

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Declarations
DECLARE @106C_HeadOfficeSegment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Head Office Segment');
DECLARE @106C_TradingSegment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Trading Segment');
DECLARE @106C_Export INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Export');
DECLARE @106C_Cereals INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Cereals');
DECLARE @106C_Pulses INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Pulses');
DECLARE @106C_Oilseeds INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Oilseeds');
DECLARE @106C_Import INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Import');
DECLARE @106C_SpareParts INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Spare Parts');
DECLARE @106C_Medicine INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Medicine');
DECLARE @106C_Construction INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Construction');
DECLARE @106C_FoodItems INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'FoodItems');
DECLARE @106C_Minidor INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Minidor');
DECLARE @106C_OilMills INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Oil Mills');
DECLARE @106C_RealEstateSegment INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Real Estate Segment');
DECLARE @106C_Rental INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Rental');
DECLARE @106C_RentalAdama INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Rental - Adama');
DECLARE @106C_RentalAA INT = (SELECT [Id] FROM dbo.[Centers] WHERE [Name] = N'Rental - AA');