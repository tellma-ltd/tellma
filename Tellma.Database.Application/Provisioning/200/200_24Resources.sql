	SET @DefinitionID = @EmployeeBenefitRD; DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[Name],						[UnitId], [CurrencyId]) VALUES
	(0,	N'Basic',					@wmo,		@ETB),
	(1, N'Transportation Allowance',@wmo,		@ETB),
	(2, N'Day Overtime',			@hr,		@ETB),
	(3, N'Night Overtime',			@hr,		@ETB),
	(4, N'Rest Overtime',			@hr,		@ETB),
	(5, N'Holiday Overtime',		@hr,		@ETB),
	(6, N'Labor (hourly)',			@hr,		@ETB),
	(7, N'Labor (daily)',			@wd,		@ETB),
	(8, N'Data package',			@wmo,		@ETB),
	(9, N'SS Contribution (11%)',	@wmo,		@ETB),
	(10,N'Meal Allowance',			@wmo,		@ETB);

	EXEC [api].[Resources__Save] -- N'employee-benefits'
		@DefinitionId = @DefinitionID,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting employee benefits: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
	
	SET @DefinitionID = @RevenueServiceRD; DELETE FROM @Resources; DELETE FROM @ResourceUnits;
		INSERT INTO @Resources ([Index],
		[Name],						[UnitId],[CurrencyId]) VALUES
	(0,	N'Monthly Subscription',	@mo,		@ETB),
	(1, N'Yearly Support',			@yr,		@ETB),
	(2, N'ERP Implementation',		@ea,		@ETB),
	(3, N'ERP Stabilization',		@mo,		@ETB)	
	;

EXEC [api].[Resources__Save] -- N'services-expenses'
	@DefinitionId = @DefinitionID,
	@Entities = @Resources,
	@ResourceUnits = @ResourceUnits,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting services: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

SELECT @MonthlySubscription = [Id] FROM dbo.[Resources] WHERE [Name] = N'Monthly Subscription';
SELECT @BasicSalary = [Id] FROM dbo.[Resources] WHERE [Name] = N'Basic';
SELECT @TransportationAllowance = [Id] FROM dbo.[Resources] WHERE [Name] = N'Transportation Allowance';
SELECT @DataPackage = [Id] FROM dbo.[Resources] WHERE [Name] = N'Data Package';
SELECT @MealAllowance = [Id] FROM dbo.[Resources] WHERE [Name] = N'Meal Allowance';
SELECT @HourlyWage = [Id] FROM dbo.[Resources] WHERE [Name] = N'Labor (hourly)';