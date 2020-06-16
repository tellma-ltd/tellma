DECLARE @Centers dbo.CenterList; 

IF @DB = N'100' -- ACME, USD, en/ar/zh
INSERT INTO @Centers([Index],
			[Name],				[Name2],			[Name3],			[Code],	[CenterType], [ParentIndex])
SELECT 0,[ShortCompanyName],[ShortCompanyName2],	[ShortCompanyName3],N'',	N'Segment',			NULL
FROM dbo.Settings
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

--	UPDATE @Centers SET [isLeaf] = 0 WHERE [Code] IN (N'', N'1', N'2');
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @Centers([Index],[ParentIndex],
			[Name],						[Code],[CenterType]) VALUES
	(1,NULL,N'Unallocated',				N'00',	N'Common'),
	(2,NULL,N'Support Servies',			N'01',	N'ServicesExtension'),
	(3,NULL,N'Selling and Gen. Admin',	N'10',	N'Abstract'),
	(4,3,	N'Shared Admin',			N'11',	N'AdministrativeExpense'),
	(5,3,	N'Shared S&D',				N'12',	N'DistributionCosts'),	
	(6,NULL,N'Projects',				N'20',	N'Abstract'),
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
	(1,NULL,N'Unallocated',				N'00',	N'Abstract'),-- expenses to be allocated
	(2,N'Exec Office',				N'11',	N'Cost',		0), -- Badege
	(3,N'Finance Dept',				N'12',	N'Cost',		0), -- Tizita
	(4,N'Marketing & Sales Dept',	N'13',	N'Cost',		0), -- Ashenafi
	(5,N'Service Centers',			N'2',	NULL,			0),
	(6,N'HR Dept',					N'21',	N'Cost',		5), -- Belay. Service: => reallocate to O/H of depts
	(7,N'Canteen',					N'22',	N'Cost',		5), -- Belay
	(8,N'Maintenance Dept',			N'23',	N'Cost',		5), -- Girma
	(9,N'Materials',				N'24',	N'Cost',		5), -- Ayelech
	(10,N'Steel Business Unit',		N'3',	N'Profit',		0), -- Badege
	(11,N'Steel Sales',				N'31',	N'Revenue',		10), -- Ashenafi
	(12,N'Steel Sales - AG',		N'311',	N'Revenue',		11), -- Ashenafi
	(13,N'Steel Sales - Bole',		N'312',	N'Revenue',		11), -- Office Manager
	(14,N'Production Dept',			N'32',	N'Cost',		10), -- Mesfin
	(15,N'Slitting Dept',			N'321',	NULL,			14),
	(16,N'HSP Dept',				N'322',	NULL,			14),
	(17,N'Cut to Size Dept',		N'323',	NULL,			14),
	(18,N'Coffee',					N'4',	N'Profit',		0), -- Gadissa
	(19,N'T/H Bldg',				N'5',	N'Profit',		0); -- Bldg Manager
	UPDATE @Centers SET [isLeaf] = 0 WHERE [Index] IN (0, 5, 10, 11, 14);
/*
	Unallocated is cleared as follows:
	Dr. Finance				Salaries	20
	Dr.	HR					Salaries	30
	Dr. HSP Dept			Salaries	100
		Cr. Unallocated		Salaries	(150) hrs Other Expense by Function (to be emptied)
		
	For each of the centers, management policy may choose to re-apportion to production centers in a certain way
	Dr. Sales Dept			Sales O/H 200 -- by number of employees in each
	Dr. Slitting Dept		Prod O/H 700
	Dr. HSP Dept			Prod O/H 800	
	Dr. CTS Dept			Prod O/H 300	
		Cr. HR Dept			Prod O/H (2,000)	-- total expenses: salaries, rent, etc

	for production event in Slitting
	Dr.	Steel BU			WIP				Strips
		Cr. HSP				Salaries		Labor
		Cr. Steel BU		Raw Materials	CR			Tons
		Cr. HSP	Dept		Prod O/H			Value (based on "hours or USD" from salaries or "tons or USD" from strips)

	for sale
	Dr.	 Steel BU			Cost of Sales
		Cr.	 Steel BU		Finished Goods
	Dr.	 Steel BU			Trade Receivable
		Cr.	 Steel BU		Revenues

	for asset purchase
	Dr.	Walia SI			PPE
		Cr. Walia SI		PPE

	for depreciation
	Dr.	Unallocated			Depreciation Expenses	Other Expense by Function (to be emptied)
		Cr. Walia SI		PPE						Accumulated Depreciation

*/
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @Centers([Index],
				[Name],				[Name2],			[Code], [CenterType], [ParentIndex])
	SELECT 0,[ShortCompanyName],[ShortCompanyName2],	N'',	N'Segment',			NULL
	FROM dbo.Settings

	INSERT INTO @Centers([Index],
		[Name],						[Name2],					[Code], [CenterType], [ParentIndex]) VALUES -- HasBS, HasRevenues, HasExpenses 
	(1,N'Simpex - Exec Office',		N'سيمبكس - مكتب تنفيذي',	N'0',	N'Segment',			0), -- ADM
	(2,N'Jeddah Branch',			N'فرع جدة',					N'1',	N'Profit',				0), -- 
	(3,N'Jeddah - Admin/Shared',	N'جدة - مكتب تنفيذي',		N'10',	N'Cost',				2), -- ADM
	(4,N'Jeddah - Sales',			N'جدة - مبيعات',			N'11',	N'Revenue',				2), -- ADM, SND
	(5,N'Jeddah - Stores',			N'جدة - مخازن',				N'12',	N'Cost',				2), -- SND
	(6,N'Riyadh Branch',			N'فرع الرياض',				N'2',	N'Profit',				0), --
	(7,N'Riyadh - Admin/Shared',	N'الرياض - مكتب تنفيذي',	N'20',	N'Cost',				6), -- ADM
	(8,N'Riyadh - Sales',			N'الرياض - مبيعات',		N'21',	N'Revenue',				6),-- ADM, SND
	(9,N'Riyadh - Stores',			N'الرياض - مخازن',			N'22',	N'Cost',				6),
	(10,N'Dammam Branch',			N'فرع الدمام',				N'3',	N'Profit',				0), --
	(11,N'Dammam - Admin/Shared',	N'الدمام - مكتب تنفيذي',	N'30',	N'Cost',				10), -- ADM
	(12,N'Dammam - Sales',			N'الدمام - مبيعات',		N'31',	N'Revenue',				10),-- ADM, SND
	(13,N'Dammam - Stores',			N'الدمام - مخازن',			N'32',	N'Cost',				10),
	(14,N'Human Resources',			N'الموارد البشرية',		N'4',	N'Cost',				0),
	(15,N'Finance',					N'الشؤون المالية',			N'5',	N'Cost',				0) -- ADM
	;

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

SELECT @RC_Inv = [Id] FROM dbo.[Centers] WHERE [CenterType] = N'Segment';

	--(1,0,N'Unallocated',		N'غ مخصص',				N'00',	N'Segment',	NULL),
	--(2,0,N'Departments',		N'الإدارات',				N'1',	N'Cost',		NULL),
	--(3,2,N'Executive Office',	N'المكتب التنفيذي',	N'11',	N'Cost',		@AdministrativeExpense),
	--(4,2,N'Sales Unit',		N'التسويق والمبيعات',	N'12',	N'Cost',		@DistributionCosts),
	--(5,2,N'System Admin',		N'إدارة النظم',			N'13',	N'Cost',		@ServiceExtension),
	--(6,2,N'Power Gen.',		N'إنتاج الكهرباء',		N'14',	N'Cost',		@ServiceExtension),
	--(7,0,N'Products',			N'المنتجات',			N'2',	N'Profit',		NULL),
	--(8,7,N'B10/HCM',			N'بابل',				N'21',	N'Profit',		@CostOfSales), -- should we say: ExpenseByFunctionExtension
	--(9,7,N'BSmart',			N'بيسمارت',				N'22',	N'Profit',		@CostOfSales),
	--(10,7,N'Campus',			N'كامبوس',				N'23',	N'Profit',		@CostOfSales),
	--(11,7,N'Tellma',			N'تلما',				N'24',	N'Profit',		@CostOfSales),
	--(12,0,N'1st Floor',		N'ط - 1',				N'30',	N'Profit',		@CostOfSales);

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