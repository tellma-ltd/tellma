-- We look at the specialized Excel files in the IT department, and we define add Resource definitions accordingly
IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	DECLARE @ComputerEquipmentId INT = (SELECT Id FROM dbo.AccountTypes WHERE Code = N'ComputerEquipmentMemberExtension');
	   
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[AccountTypeId],	[Name],								[Identifier],	[Lookup1Id],												[Lookup2Id]) VALUES
	(0,@ComputerEquipmentId,N'Microsoft Surface Pro (899 GBP)',	N'FZ889123',	dbo.fn_Lookup(N'it-equipment-manufacturers', N'Microsoft'),	dbo.fn_Lookup(N'operating-systems', N'Windows 10')),
	(1,@ComputerEquipmentId,N'Lenovo Laptop',					N'SS9898224',	dbo.fn_Lookup(N'it-equipment-manufacturers', N'Lenovo'),	dbo.fn_Lookup(N'operating-systems', N'Windows 10')),
	(2,@ComputerEquipmentId,N'Lenovo Ideapad S145',				N'100022311',	dbo.fn_Lookup(N'it-equipment-manufacturers', N'Lenovo'),	dbo.fn_Lookup(N'operating-systems', N'Windows 10')),
	(3,@ComputerEquipmentId,N'Abdulrahman Used Laptop',			N'100022312',	NULL,														dbo.fn_Lookup(N'operating-systems', N'Windows 10'));

	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
			[UnitId],					[Multiplier]) VALUES
	(0, 0, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 1, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 2, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 3, dbo.fn_UnitName__Id(N'yr'),	1)
	;
	
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
END