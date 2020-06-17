DECLARE @Centers dbo.CenterList; 

IF @DB = N'100' -- ACME, USD, en/ar/zh
	INSERT INTO @Centers([Index],[ParentIndex],
		[Name],					[Name2],				[Code],[CenterType]) VALUES
	(0,NULL,N'Unallocated',		N'غ مخصص',				N'00',	N'Common');
-- Purchasing - Hiring - Stocking - Manufacturing - Development - Marketing & Selling - Administration
ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @Centers([Index],[ParentIndex],
		[Name],					[Name2],				[Code],[CenterType]) VALUES
	(0,NULL,N'Unallocated',		N'غ مخصص',				N'00',	N'Common'),
	(1,NULL,N'Departments',		N'الإدارات',				N'10',	N'Abstract'),
	(2,1,	N'Exec. Office',	N'المكتب التنفيذي',	N'11',	N'AdministrativeExpense'),
	(3,1,	N'Sales Unit',		N'التسويق والمبيعات',	N'12',	N'DistributionCosts'),
	(4,1,	N'Services Unit',	N'وحدة الخدمات',		N'13',	N'ServicesExtension'), -- Rent, Power, and IT support
	(5,NULL,N'Profit Centers',	N'مراكز الإيرادات',		N'20',	N'Abstract'),
	(6,5,	N'B10/HCM',			N'بابل',				N'21',	N'CostOfSales'),
	(7,5,	N'BSmart',			N'بيسمارت',				N'22',	N'CostOfSales'),
	(8,5,	N'Campus',			N'كامبوس',				N'23',	N'CostOfSales'),
	(9,5,	N'Tellma',			N'تلما',				N'24',	N'CostOfSales'),
	(10,5,	N'1st Floor',		N'ط - 1',				N'29',	N'CostOfSales');
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @Centers([Index],[ParentIndex],
			[Name],						[Code],[CenterType]) VALUES
	(1,NULL,N'Unallocated',				N'000',	N'Common'),
	(2,NULL,N'Support Servies',			N'001',	N'ServicesExtension'),
	(3,NULL,N'Selling and Gen. Admin',	N'100',	N'Abstract'),
	(4,3,	N'Shared Admin',			N'101',	N'AdministrativeExpense'),
	(5,3,	N'Shared S&D',				N'102',	N'DistributionCosts'),	
	(6,NULL,N'Projects',				N'200',	N'Abstract'),
	(7,6,	N'Lifan Motors',			N'201',	N'CostOfSales'),
	(8,6,	N'Sesay',					N'202',	N'CostOfSales'),
	(9,6,	N'Soreti',					N'203',	N'CostOfSales');
END
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	INSERT INTO @Centers([Index],[ParentIndex],
		[Name],							[Code],[CenterType]) VALUES
	(1,NULL,N'Unallocated',				N'00',	N'Common');

ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @Centers([Index],[ParentIndex],
			[Name],						[Code], [CenterType]) VALUES
	(0,NULL,N'Unallocated',				N'000',	N'Common'),
	(1,NULL,N'Administration',			N'100',	N'Abstract'),
	(2,1,	N'Exec Office',				N'101',	N'AdministrativeExpense'), -- Badege
	(3,1,	N'Finance Dept',			N'102',	N'AdministrativeExpense'), -- Tizita
	(4,NULL,N'Marketing and Sales',		N'200',	N'Abstract'),
	(5,4,	N'AG Branch',				N'201',	N'DistributionCosts'), -- Ashenafi
	(6,4,	N'Bole Sales Office',		N'202',	N'DistributionCosts'), -- Ashenafi
	(7,NULL,N'Service Centers',			N'300',	N'Abstract'), -- reallocate to O/H of depts based on cited basis
	(8,7,	N'HR Dept',					N'301',	N'ServicesExtension'), -- Belay, by number of employees
	(9,7,	N'Cafeteria',				N'302',	N'ServicesExtension'), -- Belay, by number of employees
	(10,7,	N'Maintenance Dept',		N'303',	N'ServicesExtension'), -- Girma, by number of maintenance requests or by Direct Hours
	(11,7,	N'Materials',				N'304',	N'ServicesExtension'), -- Ayelech, by number of Purchase Orders
	(12,NULL,N'Production',				N'400',	N'Abstract'), -- Mesfin
	(13,12,	N'Slitting Dept',			N'401',	N'ProductionExtension'),
	(14,12,	N'HSP Dept',				N'402',	N'ProductionExtension'),
	(15,12,	N'Cut to Size Dept',		N'403',	N'ProductionExtension'),	
	(16,NULL,N'Profit Centers',			N'500',	N'Abstract'), -- Mesfin	
	(17,16,	N'Steel Sales',				N'510',	N'Abstract'), -- Ashenafi
	(18,17,	N'Steel Sales - AG',		N'511',	N'CostOfSales'), -- Ashenafi
	(19,17,	N'Steel Sales - Bole',		N'512',	N'CostOfSales'), -- Office Manager
	(20,16,	N'Other Income',			N'520',	N'Abstract'), -- 
	(21,20,	N'Coffee',					N'521',	N'Profit'), -- Gadissa
	(22,20,	N'T/H Bldg',				N'522',	N'Profit'); -- Bldg Manager
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @Centers([Index],[ParentIndex],
				[Name],						[Name2],					[Code],[CenterType]) VALUES
	(0,NULL,	N'Unallocated',				N'غ مخصص',					N'00',	N'Common'),
	(1,NULL,	N'Admin. Centers',			N'المراكز الإدارية',		N'10',	N'Abstract'),
	(2,1,		N'Exec. Office',			N'المكتب التنفيذي',		N'11',	N'AdministrativeExpense'),
	(3,1,		N'Finance',					N'الإدارة المالية',			N'12',	N'AdministrativeExpense'),
	(4,1,		N'Legal',					N'الشؤون القانونية',		N'13',	N'AdministrativeExpense'),
	(5,NULL,	N'S&D Centers',				N'المراكز التسويقية',		N'20',	N'Abstract'),
	(6,5,		N'JED - Marketing & Sales',	N'إدارة المبيعات - جدة',	N'21',	N'DistributionCosts'),
	(7,5,		N'Riyadh Branch',			N'فرع الرياض',				N'22',	N'DistributionCosts'),
	(8,5,		N'Dammam Branch',			N'فرع الدمام',				N'23',	N'DistributionCosts'),
	(10,NULL,	N'Departments',				N'المراكز الخدمية',		N'30',	N'Abstract'),
	(11,10,		N'Human Resources',			N'الموارد البشرية',		N'31',	N'ServicesExtension'),
	(12,10,		N'IT',						N'تقنية المعلومات',		N'32',	N'ServicesExtension'),
	(20,NULL,	N'Profit Centers',			N'مراكز الإيرادات',			N'40',	N'Abstract'),
	(21,5,		N'Jeddah Sales',			N'مبيعات جدة',				N'41',	N'CostOfSales'),
	(22,5,		N'Riyadh Sales',			N'مبيعات الرياض',			N'42',	N'CostOfSales'),
	(23,5,		N'Dammam Sales',			N'مبيعات الدمام',			N'43',	N'CostOfSales')
END

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DECLARE @RC_ExecutiveOffice INT, @RC_HR INT, @RC_Materials INT,	@RC_Production INT, @RC_Finance INT,
		@RC_SalesAG INT, @RC_SalesBole INT, @RC_Inv INT;

SELECT @RC_Inv = [Id] FROM dbo.[Centers] WHERE [CenterType] = N'Common';

DECLARE @C101_INV INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'00');
DECLARE @C101_UNALLOC INT	= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'00');
DECLARE @C101_EXEC INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'11');
DECLARE @C101_Sales INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'12');
DECLARE @C101_Sys INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'13');
DECLARE @C101_PWG INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'14');
--DECLARE @C101_FD INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'15');
DECLARE @C101_B10 INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'21');
DECLARE @C101_BSmart INT	= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'22');
DECLARE @C101_Campus INT	= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'23');
DECLARE @C101_Tellma INT	= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'24');
DECLARE @C101_FFLR INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'30');

SELECT @RC_ExecutiveOffice = [Id] FROM dbo.[Centers] WHERE [Name] Like N'%Exec%';
SELECT @RC_SalesAG =  [Id] FROM dbo.[Centers] WHERE Code = N'141';
SELECT @RC_SalesBole = [Id] FROM dbo.[Centers] WHERE Code = N'142';
SELECT @RC_HR = [Id] FROM dbo.[Centers] WHERE Code = N'15';
SELECT @RC_Materials =  [Id] FROM dbo.[Centers] WHERE Code = N'16';
SELECT @RC_Production =  [Id] FROM dbo.[Centers] WHERE Code = N'171';

DECLARE @RC5_Exec INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'0');
DECLARE @RC5_JedAdmin INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'10');
DECLARE @RC5_JedSales INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'11');
DECLARE @RC5_JedStores INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'12');
DECLARE @RC5_RuhAdmin INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'20');
DECLARE @RC5_RuhSales INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'21');
DECLARE @RC5_RuhStores INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'22');
DECLARE @RC5_DamAdmin INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'30');
DECLARE @RC5_DamSales INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'31');
DECLARE @RC5_DamStores INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'32');
DECLARE @RC5_HR INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'4');
DECLARE @RC5_Finance INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'5');