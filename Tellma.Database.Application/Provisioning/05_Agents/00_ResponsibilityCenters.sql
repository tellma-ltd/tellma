/* Use Cases
Missing
	- Inserting
	- Updating
	- Deleting
	- Activating
	- Deactivating
*/
/*
WSI
	Executive Office
	Finance,
	Marketing and Sales
		Mgmt Office
		AG - Sales
		Bole - Sales
	HR
	MIS
	Production
	Maintenance
	Coffee
*/
DECLARE @ResponsibilityCenters dbo.ResponsibilityCenterList;

IF @DB = N'100' -- ACME, USD, en/ar/zh
INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
			[Name],				[Name2],			[Name3],			[Code],	[ResponsibilityType], [ParentIndex])
SELECT 0,1,[ShortCompanyName],[ShortCompanyName2],	[ShortCompanyName3],N'',	N'Investment',			NULL
FROM dbo.Settings

ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
				[Name],				[Code], [ResponsibilityType], [ParentIndex])
	SELECT 0,1,[ShortCompanyName],	N'',	N'Investment',			NULL
	FROM dbo.Settings
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
			[Name],				[Code], [ResponsibilityType], [ParentIndex])
SELECT 0,1,[ShortCompanyName],	N'',	N'Investment',			NULL
FROM dbo.Settings

ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
			[Name],				[Name2],			[Code], [ResponsibilityType], [ParentIndex])
SELECT 0,1,[ShortCompanyName],[ShortCompanyName2],	N'',	N'Investment',			NULL
FROM dbo.Settings

ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
				[Name],				[Name2],			[Code], [ResponsibilityType], [ParentIndex])
	SELECT 0,1,[ShortCompanyName],[ShortCompanyName2],	N'',	N'Investment',			NULL
	FROM dbo.Settings

	INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
		[Name],						[Code], [ResponsibilityType], [ParentIndex]) VALUES
	(1,1,N'Executive/Shared',		N'0',	N'Cost',				0),
	(2,0,N'Steel Manufacturing',	N'1',	N'Profit',				0),
	(3,1,N'Finance',				N'11',	N'Cost',				2),
	(4,0,N'Marketing & Sales',		N'12',	N'Profit',				2),
	(5,1,N'Sales/Shared',			N'120',	N'Cost',				4),	
	(6,1,N'Sales - AG',				N'121',	N'Revenue',				4),
	(7,1,N'Sales - Bole',			N'122',	N'Revenue',				4),
	(8,1,N'HR',						N'13',	N'Cost',				2),
	(9,1,N'Materials',				N'14',	N'Cost',				2),
	(10,0,N'Technical',				N'15',	N'Cost',				2),
	(12,1,N'Production',			N'151',	N'Cost',				10),
	(13,1,N'Maintenance',			N'152',	N'Cost',				10),
	(14,1,N'Coffee',				N'2',	N'Profit',				0)
	;
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
				[Name],				[Name2],			[Code], [ResponsibilityType], [ParentIndex])
	SELECT 0,1,[ShortCompanyName],[ShortCompanyName2],	N'',	N'Investment',			NULL
	FROM dbo.Settings

	INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
		[Name],						[Name2],					[Code], [ResponsibilityType], [ParentIndex]) VALUES -- HasBS, HasRevenues, HasExpenses 
	(1,1,N'Simpex - Exec Office',	N'سيمبكس - مكتب تنفيذي',	N'0',	N'Investment',			0), -- ADM
	(2,0,N'Jeddah Branch',			N'فرع جدة',					N'1',	N'Profit',				0), -- 
	(3,1,N'Jeddah - Admin/Shared',	N'جدة - مكتب تنفيذي',		N'10',	N'Cost',				2), -- ADM
	(4,1,N'Jeddah - Sales',			N'جدة - مبيعات',			N'11',	N'Revenue',				2), -- ADM, SND
	(5,1,N'Jeddah - Stores',		N'جدة - مخازن',				N'12',	N'Cost',				2), -- SND
	(6,0,N'Riyadh Branch',			N'فرع الرياض',				N'2',	N'Profit',				0), --
	(7,1,N'Riyadh - Admin/Shared',	N'الرياض - مكتب تنفيذي',	N'20',	N'Cost',				6), -- ADM
	(8,1,N'Riyadh - Sales',			N'الرياض - مبيعات',		N'21',	N'Revenue',				6),-- ADM, SND
	(9,1,N'Riyadh - Stores',		N'الرياض - مخازن',			N'22',	N'Cost',				6),
	(10,0,N'Dammam Branch',			N'فرع الدمام',				N'3',	N'Profit',				0), --
	(11,1,N'Dammam - Admin/Shared',	N'الدمام - مكتب تنفيذي',	N'30',	N'Cost',				10), -- ADM
	(12,1,N'Dammam - Sales',		N'الدمام - مبيعات',		N'31',	N'Revenue',				10),-- ADM, SND
	(13,1,N'Dammam - Stores',		N'الدمام - مخازن',			N'32',	N'Cost',				10),
	(14,1,N'Human Resources',		N'الموارد البشرية',		N'4',	N'Cost',				0),
	(15,1,N'Finance',				N'الشؤون المالية',			N'5',	N'Cost',				0) -- ADM
	;
END
EXEC [api].[ResponsibilityCenters__Save]
	@Entities = @ResponsibilityCenters,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'ResponsibilityCenters: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DECLARE @RC_ExecutiveOffice INT, @RC_HR INT, @RC_Materials INT,	@RC_Production INT, @RC_Finance INT,
		@RC_SalesAG INT, @RC_SalesBole INT, @RC_Inv INT;

SELECT @RC_Inv = [Id] FROM dbo.ResponsibilityCenters WHERE [IsLeaf] = 1 AND [ResponsibilityType] = N'Investment';

SELECT @RC_ExecutiveOffice = [Id] FROM dbo.ResponsibilityCenters WHERE [Name] Like N'%Exec%';
SELECT @RC_SalesAG =  [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'141';
SELECT @RC_SalesBole = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'142';
SELECT @RC_HR = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'15';
SELECT @RC_Materials =  [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'16';
SELECT @RC_Production =  [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'171';

DECLARE @RC5_Exec INT = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'0');
DECLARE @RC5_JedAdmin INT = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'10');
DECLARE @RC5_JedSales INT = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'11');
DECLARE @RC5_JedStores INT = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'12');
DECLARE @RC5_RuhAdmin INT = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'20');
DECLARE @RC5_RuhSales INT = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'21');
DECLARE @RC5_RuhStores INT = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'22');
DECLARE @RC5_DamAdmin INT = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'30');
DECLARE @RC5_DamSales INT = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'31');
DECLARE @RC5_DamStores INT = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'32');
DECLARE @RC5_HR INT = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'4');
DECLARE @RC5_Finance INT = (SELECT [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'5');