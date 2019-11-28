	INSERT INTO dbo.ResourceDefinitions (
		[Id],				[TitlePlural],			[TitleSingular],		[ResourceTypeParentList]) VALUES
	( N'employee-benefits',	N'Employee Benefits',	N'Employee Benefit',	N'EmployeeBenefitsExpense');

	INSERT INTO dbo.ResourceClassifications (
	[ResourceDefinitionId],	[Name],														[IsLeaf],	[Node]) VALUES
	(N'employee-benefits',	N'Short-term employee benefits expense',						0,			N'/1/'),
	(N'employee-benefits',	N'Wages and salaries',											0,			N'/1/1/'),
	(N'employee-benefits',	N'Social security contributions',								0,			N'/1/2/'),
	(N'employee-benefits',	N'Other short-term employee benefits',							0,			N'/1/3/'),
	(N'employee-benefits',	N'Post-employment benefit expense, defined contribution plans',	1,			N'/2/'),
	(N'employee-benefits',	N'Post-employment benefit expense, defined benefit plans',		1,			N'/3/'),
	(N'employee-benefits',	N'Termination benefits expense',								1,			N'/4/'),
	(N'employee-benefits',	N'Other long-term employee benefits',							1,			N'/5/')
	;--Other employee expense

	DECLARE @EmployeeBenefits [dbo].ResourceList;

	INSERT INTO @EmployeeBenefits (
	[Index], [ResourceTypeId], [ResourceClassificationId],					[Name],					[TimeUnitId]) VALUES
	(0,	N'WagesAndSalaries',	dbo.fn_RCName__Id(N'Wages and salaries'),	N'Basic',				dbo.fn_UnitName__Id(N'wmo')),
	(1, N'WagesAndSalaries',	dbo.fn_RCName__Id(N'Wages and salaries'),	N'Transportation',		dbo.fn_UnitName__Id(N'wmo')),
	(2, N'WagesAndSalaries',	dbo.fn_RCName__Id(N'Wages and salaries'),	N'Day Overtime',		dbo.fn_UnitName__Id(N'hr')),
	(3, N'WagesAndSalaries',	dbo.fn_RCName__Id(N'Wages and salaries'),	N'Night Overtime',		dbo.fn_UnitName__Id(N'hr')),
	(4, N'WagesAndSalaries',	dbo.fn_RCName__Id(N'Wages and salaries'),	N'Rest Overtime',		dbo.fn_UnitName__Id(N'hr')),
	(5, N'WagesAndSalaries',	dbo.fn_RCName__Id(N'Wages and salaries'),	N'Holiday Overtime',	dbo.fn_UnitName__Id(N'hr')),
	(6, N'WagesAndSalaries',	dbo.fn_RCName__Id(N'Wages and salaries'),	N'Labor (hourly)',		dbo.fn_UnitName__Id(N'hr')),
	(7, N'WagesAndSalaries',	dbo.fn_RCName__Id(N'Wages and salaries'),	N'Labor (daily)',		dbo.fn_UnitName__Id(N'wd')),
	(8, N'OtherShorttermEmployeeBenefits',
								dbo.fn_RCName__Id(N'Wages and salaries'),	N'Data package',		dbo.fn_UnitName__Id(N'wmo')),
	(9, N'SocialSecurityContributions',
								dbo.fn_RCName__Id(N'Wages and salaries'),	N'SS Contribution (11%)',dbo.fn_UnitName__Id(N'wmo'))
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
	
	SELECT ResourceTypeId, [Name] AS 'Employee Benefit', [TimeUnit]
	FROM rpt.Resources(@EmployeeBenefitIds);
END