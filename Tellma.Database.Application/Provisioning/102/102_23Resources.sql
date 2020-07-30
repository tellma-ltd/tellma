	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[Name],						[UnitId]) VALUES
	(0,	N'Basic',					@wmo),
	(1, N'Transportation Allowance',@wmo),
	(2, N'Day Overtime',			@hr),
	(3, N'Night Overtime',			@hr),
	(4, N'Rest Overtime',			@hr),
	(5, N'Holiday Overtime',		@hr),
	(6, N'Labor (hourly)',			@hr),
	(7, N'Labor (daily)',			@wd),
	(8, N'Data package',			@wmo),
	(9, N'SS Contribution (11%)',	@wmo),
	(10,N'Meal Allowance',			@wmo);

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
		[Name],						[UnitId]) VALUES
	(0,	N'Monthly Subscription',	@mo),
	(1, N'Yearly Support',			@yr),
	(2, N'ERP Implementation',		@ea),
	(3, N'ERP Stabilization',		@mo)	
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

SELECT @MonthlySubscription = [Id] FROM dbo.[Resources] WHERE [Name] = N'Monthly Subscription';
SELECT @BasicSalary = [Id] FROM dbo.[Resources] WHERE [Name] = N'Basic';
SELECT @TransportationAllowance = [Id] FROM dbo.[Resources] WHERE [Name] = N'Transportation Allowance';
SELECT @DataPackage = [Id] FROM dbo.[Resources] WHERE [Name] = N'Data Package';
SELECT @MealAllowance = [Id] FROM dbo.[Resources] WHERE [Name] = N'Meal Allowance';
SELECT @HourlyWage = [Id] FROM dbo.[Resources] WHERE [Name] = N'Labor (hourly)';
