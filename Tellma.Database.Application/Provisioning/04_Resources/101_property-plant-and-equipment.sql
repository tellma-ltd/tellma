IF @DB = N'101' -- Banan SD, USD, en
BEGIN
--	Defining computer equipment	   
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[AccountTypeId],	[Name],								[Identifier],	[Lookup1Id],												[Lookup2Id]) VALUES
	(0,@ComputerEquipment,N'Microsoft Surface Pro (899 GBP)',	N'FZ889123',	dbo.fn_Lookup(N'it-equipment-manufacturers', N'Microsoft'),	dbo.fn_Lookup(N'operating-systems', N'Windows 10')),
	(1,@ComputerEquipment,N'Lenovo Laptop',						N'SS9898224',	dbo.fn_Lookup(N'it-equipment-manufacturers', N'Lenovo'),	dbo.fn_Lookup(N'operating-systems', N'Windows 10')),
	(2,@ComputerEquipment,N'Lenovo Ideapad S145',				N'100022311',	dbo.fn_Lookup(N'it-equipment-manufacturers', N'Lenovo'),	dbo.fn_Lookup(N'operating-systems', N'Windows 10')),
	(3,@ComputerEquipment,N'Abdulrahman Used Laptop',			N'100022312',	NULL,														dbo.fn_Lookup(N'operating-systems', N'Windows 10'));
	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
			[UnitId],					[Multiplier]) VALUES
	(0, 0, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 1, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 2, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 3, dbo.fn_UnitName__Id(N'yr'),	1);
	EXEC [api].[Resources__Save]
		@DefinitionId = N'computer-equipment',
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
		[AccountTypeId],		[Name],						[Identifier]) VALUES
	(0, @OfficeEquipment,		N'Camera',					N'-'),
	(1, @OfficeEquipment,		N'Generator',				N'-'),
	(2, @OfficeEquipment,		N'Battery for Generator',	N'-'),
	(3, @ComputerAccessories,	N'Mouse (65.49 GBP)',		N'-'),
	(4, @ComputerAccessories,	N'Laptop Case (17.99 GBP)',	N'-'),
	(5, @ComputerAccessories,	N'Dock (140.80 GBP)',		N'-'),
	(6, @OfficeEquipment,		N'FingerPrint System',		N'-'),
	(7, @ComputerAccessories,	N'SSD for PC',				N'1'),
	(8, @ComputerAccessories,	N'SSD for PC',				N'2'),
	(9, @ComputerAccessories,	N'SSD 240 GB',				N'-'),
	(10, @OfficeEquipment,		N'Meeting Luxurious Table',	N'-'),
	(11, @OfficeEquipment,		N'Generator Auto Switch',	N'-'),
	(12, @ComputerAccessories,	N'Keyboards and Mouses',	N'-'),
	(13, @ComputerAccessories,	N'Hikvision 240GB SSD Disk',N'-'),
	(14, @OfficeEquipment,		N'Huawei Prime 7 Golden',	N'-');
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
	(0, 13, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 14, dbo.fn_UnitName__Id(N'yr'),	1);
	EXEC [api].[Resources__Save]
		@DefinitionId = N'properties-plants-and-equipment',
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (properties-plants-and-equipment): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END