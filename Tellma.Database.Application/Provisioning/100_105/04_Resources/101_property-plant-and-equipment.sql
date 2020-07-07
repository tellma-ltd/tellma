IF @DB = N'101' -- Banan SD, USD, en
BEGIN
--	Defining computer equipment	 @CostOfSales @DistributionCosts @AdministrativeExpense @ProductionExtension @ServiceExtension @OtherExpenseByFunction
	DELETE FROM @Resources;
	--INSERT INTO @Resources ([Index],
	--	[AssetTypeId],				[CurrencyId],	[ExpenseTypeId],		[CenterId],	[Name],								[Identifier],	[Lookup1Id],												[Lookup2Id]) VALUES
	--(0,@ComputerEquipmentMemberExtension,	@USD,	@DepreciationExpense,	@C101_EXEC,	N'Microsoft Surface Pro (899 GBP)',	N'FZ889123',	dbo.fn_Lookup(@ITEquipmentManufacturerLKD, N'Microsoft'),dbo.fn_Lookup(@OperatingSystemLKD, N'Windows 10')),
	--(1,@ComputerEquipmentMemberExtension,	@USD,	@DepreciationExpense,	@C101_Sales,N'Lenovo Laptop',					N'SS9898224',	dbo.fn_Lookup(@ITEquipmentManufacturerLKD, N'Lenovo'),	dbo.fn_Lookup(@OperatingSystemLKD, N'Windows 10')),
	--(2,@ComputerEquipmentMemberExtension,	@USD,	@DepreciationExpense,	@C101_Campus,N'Lenovo Ideapad S145',			N'100022311',	dbo.fn_Lookup(@ITEquipmentManufacturerLKD, N'Lenovo'),	dbo.fn_Lookup(@OperatingSystemLKD, N'Windows 10')),
	--(3,@ComputerEquipmentMemberExtension,	@USD,	@DepreciationExpense,	@C101_B10,	N'Abdulrahman Used Laptop',			N'100022312',	NULL,														dbo.fn_Lookup(@OperatingSystemLKD, N'Windows 10'));
	INSERT INTO @Resources ([Index],
		[CurrencyId],[CenterId],	[Name],								[Identifier],	[Lookup1Id],												[Lookup2Id],								[UnitId]) VALUES
	(0,	@USD,		@C101_EXEC,	N'Microsoft Surface Pro (899 GBP)',	N'FZ889123',	dbo.fn_Lookup(@ITEquipmentManufacturerLKD, N'Microsoft'),dbo.fn_Lookup(@OperatingSystemLKD, N'Windows 10'),	dbo.fn_UnitName__Id(N'yr')),
	(1,	@USD,		@C101_Sales,N'Lenovo Laptop',					N'SS9898224',	dbo.fn_Lookup(@ITEquipmentManufacturerLKD, N'Lenovo'),	dbo.fn_Lookup(@OperatingSystemLKD, N'Windows 10'), dbo.fn_UnitName__Id(N'yr')),
	(2,	@USD,		@C101_Campus,N'Lenovo Ideapad S145',			N'100022311',	dbo.fn_Lookup(@ITEquipmentManufacturerLKD, N'Lenovo'),	dbo.fn_Lookup(@OperatingSystemLKD, N'Windows 10'), dbo.fn_UnitName__Id(N'yr')),
	(3,	@USD,		@C101_B10,	N'Abdulrahman Used Laptop',			N'100022312',	NULL,														dbo.fn_Lookup(@OperatingSystemLKD, N'Windows 10'), dbo.fn_UnitName__Id(N'yr'));

	EXEC [api].[Resources__Save]
		@DefinitionId = @ComputerEquipmentMemberRD,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (computer-equipment): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
-- Defining other fixed assets
	DELETE FROM @Resources;
INSERT INTO @Resources ([Index],
		[Name],				[Identifier],[CurrencyId],	[CenterId]) VALUES
	(0, N'Camera',					N'-', @USD,			@C101_Sales),
	(1, N'Generator',				N'-', @USD,			@C101_PWG),
	(2, N'Battery for Generator',	N'-', @USD,			@C101_PWG),
	(3, N'Mouse (65.49 GBP)',		N'-', @USD,			@C101_EXEC),
	(4, N'Laptop Case (17.99 GBP)',	N'-', @USD,			@C101_EXEC),
	(5, N'Dock (140.80 GBP)',		N'-', @USD,			@C101_EXEC),
	(6, N'FingerPrint System',		N'-', @USD,			@C101_EXEC),
	(7, N'SSD for PC',				N'1', @USD, 		@C101_B10),
	(8, N'SSD for PC',				N'2', @USD, 		@C101_Campus),
	(9, N'SSD 240 GB',				N'-', @USD, 		@C101_Sys),
	(10,N'Meeting Luxurious Table',	N'-', @USD, 		@C101_EXEC),
	(11,N'Generator Auto Switch',	N'-', @USD, 		@C101_PWG),
	(12,N'Hikvision 240GB SSD Disk',N'-', @USD, 		@C101_B10),
	(13,N'Huawei Prime 7 Golden',	N'-', @USD, 		@C101_Sales);

	EXEC [api].[Resources__Save]
		@DefinitionId = @OtherPropertyPlantAndEquipmentMemberRD,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (properties-plants-and-equipment): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END