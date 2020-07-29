
IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @Centers([Index],[ParentIndex],
			[Name],						[Code],[CenterType]) VALUES
	(0,NULL,N'Banan',					N'0',	N'Abstract'),
	(1,0,	N'AA Branch',				N'1',	N'BusinessUnit'), -- all except revenues and expenses by nature (because not a leaf)
	(10,1,	N'Selling and Gen. Admin',	N'10',	N'SellingGeneralAndAdministration'), -- expenses by naturem and
	
	(2,0,	N'Profit Centers',			N'2',	N'Abstract'),
	(20,2,	N'Overhead',				N'20',	N'SharedExpenseControl'), -- expense by nature
	(21,2,	N'Lifan Motors',			N'21',	N'BusinessUnit'), -- all accounts
	(22,2,	N'Sesay',					N'22',	N'BusinessUnit'),
	(23,2,	N'Soreti',					N'23',	N'BusinessUnit');
	-- Technically, when a project crosses a year boundary, we need to capitalize the expenses, and only recognize them
	-- when the client approves the deliverables. However, in ET, this may complicate tax declaration, so we will not do it.
	--(3,0,	N'ERP Projects',			N'3',	N'Abstract'),
	--(31,3,	N'Lifan Motors Project',	N'31',	N'ProductionExtension'), -- expense by nature
	--(32,3,	N'Sesay Project',			N'32',	N'ProductionExtension'),
	--(33,3,	N'Soreti Project',			N'33',	N'ProductionExtension');

END
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @Centers([Index],[ParentIndex],
			[Name],						[Code], [CenterType]) VALUES
	(0,NULL,N'Walia Steel',				N'0',	N'Abstract'),

	(1,0,	N'Headquarters',			N'1',	N'BusinessUnit'),
	(100,1,	N'Common',					N'100',	N'SellingGeneralAndAdministration'), -- Badege
	(110,1,	N'Exec Office',				N'110',	N'SellingGeneralAndAdministration'), -- Badege
	(120,1,	N'Finance Dept',			N'120',	N'SellingGeneralAndAdministration'), -- Tizita

	(13,1,	N'Marketing and Sales',		N'13',	N'Abstract'),
	(130,13,N'Marketing and Sales Mgmt',N'130',	N'SellingGeneralAndAdministration'), -- Ashenafi
	(131,13,N'AG Office',				N'131',	N'SellingGeneralAndAdministration'), -- Ashenafi
	(132,13,N'Bole Office',				N'132',	N'SellingGeneralAndAdministration'), -- Ashenafi

	(14,1,	N'Service Centers',			N'14',	N'Abstract'), -- reallocate to O/H of depts based on cited basis
	(141,14,N'HR Dept',					N'141',	N'SharedExpenseControl'), -- Belay, by number of employees
	(142,14,N'Cafeteria',				N'142',	N'SharedExpenseControl'), -- Belay, by number of employees
	(143,14,N'Maintenance Dept',		N'143',	N'SharedExpenseControl'), -- Girma, by number of maintenance requests or by Direct Hours
	(144,14,N'Materials',				N'144',	N'SharedExpenseControl'), -- Ayelech, by number of Purchase Orders

	(2,0,	N'Steel',					N'2',	N'BusinessUnit'),
	(200,2,	N'Steel - Sales',			N'200',	N'CostOfSales'),
	(21,2,	N'Production',				N'21',	N'Abstract'),
	(210,21,N'Production Management',	N'210',	N'WorkInProgressExpendituresControl'),
	(211,21,N'Slitting Dept',			N'211',	N'WorkInProgressExpendituresControl'),
	(212,21,N'HSP Dept',				N'212',	N'WorkInProgressExpendituresControl'),
	(213,21,N'Cut to Size Dept',		N'213',	N'WorkInProgressExpendituresControl'),	

	(3,0,	N'Other Income',			N'3',	N'Abstract'), -- 
	(301,3,	N'T/H Bldg',				N'301',	N'BusinessUnit'), -- Bldg Manager
	(302,3,	N'Coffee',					N'302',	N'BusinessUnit'), -- Gadissa
	(399,3,	N'Misc.',					N'399',	N'BusinessUnit'); -- Gadissa


END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @Centers([Index],[ParentIndex],
				[Name],						[Name2],					[Code],[CenterType]) VALUES
	(0,NULL,	N'Simpex',					N'سيمبكس',					N'0',	N'Abstract'),
	(1,NULL,	N'Headquarters',			N'الرئاسة',					N'1',	N'BusinessUnit'),
	(10,1,		N'Common Expenses',			N'المصروفات العمومية',		N'10',	N'SellingGeneralAndAdministration'),
	(11,1,		N'Exec. Office',			N'المكتب التنفيذي',		N'11',	N'SellingGeneralAndAdministration'),
	(12,1,		N'Finance',					N'الإدارة المالية',			N'12',	N'SellingGeneralAndAdministration'),
	(13,1,		N'Legal',					N'الشؤون القانونية',		N'13',	N'SellingGeneralAndAdministration'),
	(14,1,		N'Human Resources',			N'الموارد البشرية',		N'14',	N'SharedExpenseControl'),
	(15,1,		N'IT',						N'تقنية المعلومات',		N'15',	N'SharedExpenseControl'),
	(2,0,		N'Branches',				N'الفروع',					N'2',	N'Abstract'),
	(21,2,		N'Jeddah Branch',			N'فرع جدة',					N'21',	N'BusinessUnit'),
	(22,2,		N'Riyadh Branch',			N'فرع الرياض',				N'22',	N'BusinessUnit'),
	(23,2,		N'Dammam Branch',			N'فرع الدمام',				N'23',	N'BusinessUnit')
END

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

--DECLARE @RC_ExecutiveOffice INT, @RC_HR INT, @RC_Materials INT,	@RC_Production INT, @RC_Finance INT,
--		@RC_SalesAG INT, @RC_SalesBole INT, @RC_Inv INT;

--SELECT @RC_Inv = [Id] FROM dbo.[Centers] WHERE [CenterType] = N'Common';

--DECLARE @C101_INV INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'00');
--DECLARE @C101_UNALLOC INT	= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'00');
--DECLARE @C101_EXEC INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'11');
--DECLARE @C101_Sales INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'12');
--DECLARE @C101_Sys INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'13');
--DECLARE @C101_PWG INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'14');
----DECLARE @C101_FD INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'15');
--DECLARE @C101_B10 INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'21');
--DECLARE @C101_BSmart INT	= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'22');
--DECLARE @C101_Campus INT	= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'23');
--DECLARE @C101_Tellma INT	= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'24');
--DECLARE @C101_FFLR INT		= (SELECT [Id] FROM dbo.Centers WHERE [Code] = N'30');

--SELECT @RC_ExecutiveOffice = [Id] FROM dbo.[Centers] WHERE [Name] Like N'%Exec%';
--SELECT @RC_SalesAG =  [Id] FROM dbo.[Centers] WHERE Code = N'141';
--SELECT @RC_SalesBole = [Id] FROM dbo.[Centers] WHERE Code = N'142';
--SELECT @RC_HR = [Id] FROM dbo.[Centers] WHERE Code = N'15';
--SELECT @RC_Materials =  [Id] FROM dbo.[Centers] WHERE Code = N'16';
--SELECT @RC_Production =  [Id] FROM dbo.[Centers] WHERE Code = N'171';

--DECLARE @RC5_Exec INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'0');
--DECLARE @RC5_JedAdmin INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'10');
--DECLARE @RC5_JedSales INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'11');
--DECLARE @RC5_JedStores INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'12');
--DECLARE @RC5_RuhAdmin INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'20');
--DECLARE @RC5_RuhSales INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'21');
--DECLARE @RC5_RuhStores INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'22');
--DECLARE @RC5_DamAdmin INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'30');
--DECLARE @RC5_DamSales INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'31');
--DECLARE @RC5_DamStores INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'32');
--DECLARE @RC5_HR INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'4');
--DECLARE @RC5_Finance INT = (SELECT [Id] FROM dbo.Centers WHERE Code = N'5');