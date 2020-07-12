DELETE FROM @Resources;
	INSERT INTO @Resources ([Index],
		[CurrencyId],[CenterId],	[Name],								[Identifier],	[Lookup1Id],	[Lookup2Id],	[UnitId]) VALUES
	(0,	@USD,		@101CHQ,		N'Microsoft Surface Pro (899 GBP)',	N'FZ889123',	@MicrosoftLKP,	@Windows10LKP,	@yr),
	(1,	@USD,		@101CSales,		N'Lenovo Laptop',					N'SS9898224',	@LenovoLKP,		@Windows10LKP,	@yr),
	(2,	@USD,		@101CCampus,	N'Lenovo Ideapad S145',				N'100022311',	@LenovoLKP,		@Windows10LKP,	@yr),
	(3,	@USD,		@101CB10,		N'Abdulrahman Used Laptop',			N'100022312',	NULL,			@Windows10LKP,	@yr);

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
	(0, N'Camera',					N'-', @USD,			@101CSales),
	(1, N'Generator',				N'-', @USD,			@101CServices),
	(2, N'Battery for Generator',	N'-', @USD,			@101CServices),
	(3, N'Mouse (65.49 GBP)',		N'-', @USD,			@101CEO),
	(4, N'Laptop Case (17.99 GBP)',	N'-', @USD,			@101CEO),
	(5, N'Dock (140.80 GBP)',		N'-', @USD,			@101CEO),
	(6, N'FingerPrint System',		N'-', @USD,			@101CServices),
	(7, N'SSD for PC',				N'1', @USD, 		@101CB10),
	(8, N'SSD for PC',				N'2', @USD, 		@101CCampus),
	(9, N'SSD 240 GB',				N'-', @USD, 		@101CServices),
	(10,N'Meeting Luxurious Table',	N'-', @USD, 		@101CEO),
	(11,N'Generator Auto Switch',	N'-', @USD, 		@101CServices),
	(12,N'Hikvision 240GB SSD Disk',N'-', @USD, 		@101CB10),
	(13,N'Huawei Prime 7 Golden',	N'-', @USD, 		@101CSales);

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

	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
			[Name],				[UnitId]) VALUES
	(0,		N'Basic',			@wmo),
	(1,		N'Labor (hourly)',	@hr);

	EXEC [api].[Resources__Save] -- N'employee-benefits'
		@DefinitionId = @EmployeeBenefitRD,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting employee benefits: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
	
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
		INSERT INTO @Resources ([Index],
		[Name],						[Name2],			[UnitId]) VALUES
	(0,	N'Monthly Subscription',	N'اشتراك شهري',		@mo),
	(1, N'Yearly Support',			N'مساندة سنوية',	@yr),
	(2, N'ERP Implementation',		N'تفعيل النظام',	@ea),
	(3, N'ERP Stabilization',		N'استقرار النظام',	@mo)	
	;

EXEC [api].[Resources__Save] -- N'services-expenses'
	@DefinitionId = @RevenueServiceRD,
	@Entities = @Resources,
	@ResourceUnits = @ResourceUnits,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting services: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

SELECT @MonthlySubscription = [Id] FROM dbo.Resources WHERE [Name] = N'Monthly Subscription';
SELECT @BasicSalary = [Id] FROM dbo.Resources WHERE [Name] = N'Basic';
SELECT @TransportationAllowance = [Id] FROM dbo.Resources WHERE [Name] = N'Transportation Allowance';
SELECT @DataPackage = [Id] FROM dbo.Resources WHERE [Name] = N'Data Package';
SELECT @MealAllowance = [Id] FROM dbo.Resources WHERE [Name] = N'Meal Allowance';
SELECT @HourlyWage = [Id] FROM dbo.Resources WHERE [Name] = N'Labor (hourly)';