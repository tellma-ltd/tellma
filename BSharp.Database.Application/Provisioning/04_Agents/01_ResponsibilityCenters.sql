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
DECLARE @OS_Steel INT, @RC_ExecutiveOffice INT, @RC_HR INT, @RC_Materials INT,	@RC_Production INT, @RC_Finance INT,
		@RC_SalesAG INT, @RC_SalesBole INT, @OS_IT INT, @OS_CarAssembly INT;

IF @DB = N'100' -- ACME, USD, en/ar/zh
INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
	[Name],							[Code], [ResponsibilityType], [ParentIndex]) VALUES
(0,1,N'ACME',						N'',	N'Investment',			NULL);

ELSE IF @DB = N'101' -- Banan SD, USD, en
INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
	[Name],							[Code], [ResponsibilityType], [ParentIndex]) VALUES
(0,0,N'Banan IT',					N'',	N'Investment',			NULL),
(1,1,N'Technical',					N'1',	N'Cost',				0),
(2,1,N'Sales',						N'2',	N'Revenue',				0),
(3,1,N'Executive/Shared',			N'3',	N'Cost',				0);

ELSE IF @DB = N'102' -- Banan ET, ETB, en
INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
	[Name],							[Code], [ResponsibilityType], [ParentIndex]) VALUES
(0,1,N'IT Services',				N'',	N'Investment',			NULL);

ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
	[Name],							[Code], [ResponsibilityType], [ParentIndex]) VALUES
(0,1,N'Car Assembly',				N'',	N'Investment',			NULL);

ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
	[Name],							[Code], [ResponsibilityType], [ParentIndex]) VALUES
(0,0,N'Walia Steel',				N'',	N'Investment',			NULL),
(1,1,N'Executive/Shared',			N'0',	N'Cost',				0),
(2,0,N'Steel Manufacturing',		N'1',	N'Investment',			0),
(3,1,N'Finance',					N'11',	N'Cost',				2),
(4,0,N'Marketing & Sales',			N'12',	N'Revenue',				2),
(5,1,N'Sales/Shared',				N'120',	N'Cost',				4),
(6,1,N'Sales - AG',					N'121',	N'Revenue',				4),
(7,1,N'Sales - Bole',				N'122',	N'Revenue',				4),
(8,1,N'HR',							N'13',	N'Cost',				2),
(9,1,N'Materials',					N'14',	N'Cost',				2),
(10,0,N'Technical',					N'15',	N'Cost',				2),
(12,1,N'Production',				N'151',	N'Cost',				10),
(13,1,N'Maintenance',				N'152',	N'Cost',				10),
(14,1,N'Coffee',					N'2',	N'Profit',				0),
(15,1,N'Walia Common',				N'3',	N'Cost',				0)
;
EXEC [api].[ResponsibilityCenters__Save]
	@Entities = @ResponsibilityCenters,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'ResponsibilityCenters: Inserting'
	GOTO Err_Label;
END;
IF @DebugResponsibilityCenters = 1
	SELECT * FROM [dbo].ResponsibilityCenters;
SELECT @OS_Steel = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'1';
SELECT @RC_ExecutiveOffice = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'10';
SELECT @RC_SalesAG =  [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'141';
SELECT @RC_SalesBole = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'142';
SELECT @RC_HR = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'15';
SELECT @RC_Materials =  [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'16';
SELECT @RC_Production =  [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'171';
SELECT @OS_IT = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'2';
SELECT @OS_CarAssembly = [Id] FROM dbo.ResponsibilityCenters WHERE Code = N'3';