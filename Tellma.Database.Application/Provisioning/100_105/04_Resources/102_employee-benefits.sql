IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	DELETE FROM @Resources;
	INSERT INTO @Resources ([Index],
		[Name], [UnitId]) VALUES
	(0,	N'Basic', dbo.fn_UnitName__Id(N'wmo')),
	(1, N'Transportation Allowance', dbo.fn_UnitName__Id(N'wmo')),
	(2, N'Day Overtime', dbo.fn_UnitName__Id(N'hr')),
	(3, N'Night Overtime', dbo.fn_UnitName__Id(N'hr')),
	(4, N'Rest Overtime', dbo.fn_UnitName__Id(N'hr')),
	(5, N'Holiday Overtime', dbo.fn_UnitName__Id(N'hr')),
	(6, N'Labor (hourly)', dbo.fn_UnitName__Id(N'hr')),
	(7, N'Labor (daily)', dbo.fn_UnitName__Id(N'wd')),
	(8, N'Data package', dbo.fn_UnitName__Id(N'wmo')),
	(9, N'SS Contribution (11%)', dbo.fn_UnitName__Id(N'wmo')),
	(10,N'Meal Allowance', dbo.fn_UnitName__Id(N'wmo'));

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
	
	SELECT @BasicSalary = [Id] FROM dbo.Resources WHERE [Name] = N'Basic';
	SELECT @TransportationAllowance = [Id] FROM dbo.Resources WHERE [Name] = N'Transportation Allowance';
	SELECT @DataPackage = [Id] FROM dbo.Resources WHERE [Name] = N'Data package';
	SELECT @MealAllowance = [Id] FROM dbo.Resources WHERE [Name] = N'Meal Allowance';
	SELECT @HourlyWage = [Id] FROM dbo.Resources WHERE [Name] = N'Labor (hourly)';

	SELECT @DayOvertime = [Id] FROM dbo.Resources WHERE [Name] = N'Day Overtime';
	SELECT @NightOvertime = [Id] FROM dbo.Resources WHERE [Name] = N'Night Overtime';
	SELECT @RestOvertime = [Id] FROM dbo.Resources WHERE [Name] = N'Rest Overtime';
	SELECT @HolidayOvertime = [Id] FROM dbo.Resources WHERE [Name] = N'Holiday Overtime';
END