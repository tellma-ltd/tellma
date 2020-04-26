IF @DB = N'101' -- Banan SD, USD, en
BEGIN
--	Defining computer equipment	 @CostOfSales @DistributionCosts @AdministrativeExpense @ProductionExtension @ServiceExtension @OtherExpenseByFunction
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[AssetTypeId],				[CurrencyId],	[ExpenseTypeId],		[ExpenseEntryTypeId],	[CenterId],	[Name],								[Identifier],	[Lookup1Id],												[Lookup2Id]) VALUES
	(0,@ComputerEquipmentMemberExtension,	@USD,	@DepreciationExpense,	@AdministrativeExpense,	@C101_EXEC,	N'Microsoft Surface Pro (899 GBP)',	N'FZ889123',	dbo.fn_Lookup(@it_equipment_manufacturersDef, N'Microsoft'),dbo.fn_Lookup(@operating_systemsDef, N'Windows 10')),
	(1,@ComputerEquipmentMemberExtension,	@USD,	@DepreciationExpense,	@DistributionCosts,		@C101_Sales,N'Lenovo Laptop',					N'SS9898224',	dbo.fn_Lookup(@it_equipment_manufacturersDef, N'Lenovo'),	dbo.fn_Lookup(@operating_systemsDef, N'Windows 10')),
	(2,@ComputerEquipmentMemberExtension,	@USD,	@DepreciationExpense,	@DistributionCosts,		@C101_Campus,N'Lenovo Ideapad S145',			N'100022311',	dbo.fn_Lookup(@it_equipment_manufacturersDef, N'Lenovo'),	dbo.fn_Lookup(@operating_systemsDef, N'Windows 10')),
	(3,@ComputerEquipmentMemberExtension,	@USD,	@DepreciationExpense,	@ProductionExtension,	@C101_B10,	N'Abdulrahman Used Laptop',			N'100022312',	NULL,														dbo.fn_Lookup(@operating_systemsDef, N'Windows 10'));
	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
			[UnitId],					[Multiplier]) VALUES
	(0, 0, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 1, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 2, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 3, dbo.fn_UnitName__Id(N'yr'),	1);
	EXEC [api].[Resources__Save]
		@DefinitionId = @computer_equipmentDef,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (computer-equipment): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
-- Defining other fixed assets
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[AssetTypeId],					[Name],				[Identifier],[CurrencyId],[ExpenseTypeId],	[ExpenseEntryTypeId],	[CenterId]) VALUES
	(0, @OfficeEquipment,				N'Camera',					N'-', @USD,	@DepreciationExpense,	@DistributionCosts,		@C101_Sales),
	(1, @OfficeEquipment,				N'Generator',				N'-', @USD,	@DepreciationExpense,	@ServiceExtension,		@C101_PWG),
	(2, @OfficeEquipment,				N'Battery for Generator',	N'-', @USD,	@DepreciationExpense,	@ServiceExtension,		@C101_PWG),
	(3, @ComputerAccessoriesExtension,	N'Mouse (65.49 GBP)',		N'-', @USD,	@DepreciationExpense,	@AdministrativeExpense,	@C101_EXEC),
	(4, @ComputerAccessoriesExtension,	N'Laptop Case (17.99 GBP)',	N'-', @USD,	@DepreciationExpense,	@AdministrativeExpense,	@C101_EXEC),
	(5, @ComputerAccessoriesExtension,	N'Dock (140.80 GBP)',		N'-', @USD,	@DepreciationExpense,	@AdministrativeExpense,	@C101_EXEC),
	(6, @OfficeEquipment,				N'FingerPrint System',		N'-', @USD,	@DepreciationExpense,	@AdministrativeExpense,	@C101_EXEC),
	(7, @ComputerAccessoriesExtension,	N'SSD for PC',				N'1', @USD, @DepreciationExpense,	@ProductionExtension,	@C101_B10),
	(8, @ComputerAccessoriesExtension,	N'SSD for PC',				N'2', @USD, @DepreciationExpense,	@ProductionExtension,	@C101_Campus),
	(9, @ComputerAccessoriesExtension,	N'SSD 240 GB',				N'-', @USD, @DepreciationExpense,	@ServiceExtension,		@C101_Sys),
	(10, @OfficeEquipment,				N'Meeting Luxurious Table',	N'-', @USD, @DepreciationExpense,	@AdministrativeExpense,	@C101_EXEC),
	(11, @OfficeEquipment,				N'Generator Auto Switch',	N'-', @USD, @DepreciationExpense,	@ServiceExtension,		@C101_PWG),
	(12, @ComputerAccessoriesExtension,	N'Hikvision 240GB SSD Disk',N'-', @USD, @DepreciationExpense,	@ProductionExtension,	@C101_B10),
	(13, @OfficeEquipment,				N'Huawei Prime 7 Golden',	N'-', @USD, @DepreciationExpense,	@DistributionCosts,		@C101_Sales);
	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
	[UnitId],					[Multiplier]) VALUES
	(0, 0, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 1, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 2, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 3, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 4, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 5, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 6, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 7, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 8, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 9, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 10, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 11, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 12, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 13, dbo.fn_UnitName__Id(N'yr'),	1);
	EXEC [api].[Resources__Save]
		@DefinitionId = @properties_plants_and_equipmentDef,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (properties-plants-and-equipment): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END