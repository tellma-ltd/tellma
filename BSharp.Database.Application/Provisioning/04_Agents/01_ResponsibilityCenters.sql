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

INSERT INTO @ResponsibilityCenters([Index], [IsLeaf],
	[Name],							[Code], [ResponsibilityType], [ParentIndex]) VALUES
(0,0,N'Steel Manufacturing',		N'1',	N'Investment',				NULL),
(1,1,N'Executive Office',			N'10',	N'Cost',					0),
(2,1,N'Finance',					N'12',	N'Cost',					0),
(3,0,N'Marketing & Sales',			N'13',	N'Revenue',					0),
(4,1,N'Sales Dept Mgmt Office',		N'140',	N'Cost',					3),
(5,1,N'Sales - AG',					N'141',	N'Revenue',					3),
(6,1,N'Sales - Bole',				N'142',	N'Revenue',					3),
(7,1,N'HR',							N'15',	N'Cost',					0),
(8,1,N'Materials',					N'16',	N'Cost',					0),
(9,0,N'Technical',					N'17',	N'Cost',					0),
(10,1,N'Production',				N'171',	N'Cost',					9),
(11,1,N'Maintenance',				N'172',	N'Cost',					9),
(12,1,N'IT Services',				N'2',	N'Investment',				NULL),
(13,1,N'Car Assembly',				N'3',	N'Investment',				NULL)
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