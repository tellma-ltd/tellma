	INSERT INTO dbo.ResourceDefinitions (
		[Id],				[TitlePlural],			[TitleSingular]) VALUES
	( N'employee-benefits',	N'Employee Benefits',	N'Employee Benefit');

	DECLARE @EmployeeBenefits [dbo].ResourceList;

	INSERT INTO @EmployeeBenefits (
	[Index], [ResourceClassificationId],					[Name],					[TimeUnitId]) VALUES
	--N'WagesAndSalaries'
	(0,	dbo.fn_RCCode__Id(N'WagesAndSalaries'),				N'Basic',				dbo.fn_UnitName__Id(N'wmo')),
	(1, dbo.fn_RCCode__Id(N'WagesAndSalaries'),				N'Transportation',		dbo.fn_UnitName__Id(N'wmo')),
	(2, dbo.fn_RCCode__Id(N'WagesAndSalaries'),				N'Day Overtime',		dbo.fn_UnitName__Id(N'hr')),
	(3, dbo.fn_RCCode__Id(N'WagesAndSalaries'),				N'Night Overtime',		dbo.fn_UnitName__Id(N'hr')),
	(4, dbo.fn_RCCode__Id(N'WagesAndSalaries'),				N'Rest Overtime',		dbo.fn_UnitName__Id(N'hr')),
	(5, dbo.fn_RCCode__Id(N'WagesAndSalaries'),				N'Holiday Overtime',	dbo.fn_UnitName__Id(N'hr')),
	(6, dbo.fn_RCCode__Id(N'WagesAndSalaries'),				N'Labor (hourly)',		dbo.fn_UnitName__Id(N'hr')),
	(7, dbo.fn_RCCode__Id(N'WagesAndSalaries'),				N'Labor (daily)',		dbo.fn_UnitName__Id(N'wd')),
	(8, dbo.fn_RCCode__Id(N'OtherShorttermEmployeeBenefits'),N'Data package',		dbo.fn_UnitName__Id(N'wmo')),
	(9, dbo.fn_RCCode__Id(N'SocialSecurityContributions'),	N'SS Contribution (11%)',dbo.fn_UnitName__Id(N'wmo'))
	;

	EXEC [api].[Resources__Save] -- N'employee-benefits'
		@DefinitionId = N'employee-benefits',
		@Entities = @EmployeeBenefits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting employee benefits'
		GOTO Err_Label;
	END;

IF @DebugResources = 1
BEGIN
	SELECT  N'employee-benefits' AS [Resource Definition]
	DECLARE @EmployeeBenefitIds dbo.IdList;
	INSERT INTO @EmployeeBenefitIds SELECT [Id] FROM dbo.Resources WHERE [DefinitionId] = N'employee-benefits';
	
	SELECT [Name] AS 'Employee Benefit', [TimeUnit]
	FROM rpt.Resources(@EmployeeBenefitIds);
END