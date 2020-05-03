IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	--INSERT INTO @Resources ([Index],
	--					[ExpenseTypeId],					[Name]) VALUES
	--(0,	dbo.fn_ATCode__Id(N'WagesAndSalaries'),				N'Basic'),
	--(1, dbo.fn_ATCode__Id(N'WagesAndSalaries'),				N'Transportation Allowance'),
	--(2, dbo.fn_ATCode__Id(N'WagesAndSalaries'),				N'Day Overtime'),
	--(3, dbo.fn_ATCode__Id(N'WagesAndSalaries'),				N'Night Overtime'),
	--(4, dbo.fn_ATCode__Id(N'WagesAndSalaries'),				N'Rest Overtime'),
	--(5, dbo.fn_ATCode__Id(N'WagesAndSalaries'),				N'Holiday Overtime'),
	--(6, dbo.fn_ATCode__Id(N'WagesAndSalaries'),				N'Labor (hourly)'),
	--(7, dbo.fn_ATCode__Id(N'WagesAndSalaries'),				N'Labor (daily)'),
	--(8, dbo.fn_ATCode__Id(N'OtherShorttermEmployeeBenefits'),N'Data package'),
	--(9, dbo.fn_ATCode__Id(N'SocialSecurityContributions'),	N'SS Contribution (11%)'),
	--(10, dbo.fn_ATCode__Id(N'WagesAndSalaries'),			N'Meal Allowance');
	INSERT INTO @Resources ([Index],
		[Name]) VALUES
	(0,	N'Basic'),
	(1, N'Transportation Allowance'),
	(2, N'Day Overtime'),
	(3, N'Night Overtime'),
	(4, N'Rest Overtime'),
	(5, N'Holiday Overtime'),
	(6, N'Labor (hourly)'),
	(7, N'Labor (daily)'),
	(8, N'Data package'),
	(9, N'SS Contribution (11%)'),
	(10,N'Meal Allowance');
	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
			[UnitId],						[Multiplier]) VALUES
	(0, 0, dbo.fn_UnitName__Id(N'wmo'),		1),
	(0, 1, dbo.fn_UnitName__Id(N'wmo'),		1),
	(0, 2, dbo.fn_UnitName__Id(N'hr'),		1),
	(0, 3, dbo.fn_UnitName__Id(N'hr'),		1),
	(0, 4, dbo.fn_UnitName__Id(N'hr'),		1),
	(0, 5, dbo.fn_UnitName__Id(N'hr'),		1),
	(0, 6, dbo.fn_UnitName__Id(N'hr'),		1),
	(0, 7, dbo.fn_UnitName__Id(N'wd'),		1),
	(0, 8, dbo.fn_UnitName__Id(N'wmo'),		1),
	(0, 9, dbo.fn_UnitName__Id(N'wmo'),		1),
	(0, 10, dbo.fn_UnitName__Id(N'wmo'),	1);

	EXEC [api].[Resources__Save] -- N'employee-benefits'
		@DefinitionId = @employee_benefits_expensesDef,
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