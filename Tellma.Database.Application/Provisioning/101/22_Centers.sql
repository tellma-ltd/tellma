	DELETE FROM @Centers;
	INSERT INTO @Centers([Index],[ParentIndex],
		[Name],					[Name2],					[Code],[CenterType]) VALUES
	(0,NULL,N'Banan',			N'بنان',					N'0',	N'Abstract'),
	
	(1,0,	N'Headquarters',	N'الرئاسة',					N'1',	N'BusinessUnit'),
	(11,1,	N'Exec. Office',	N'المكتب التنفيذي',		N'11',	N'SellingGeneralAndAdministration'),-- only expense by nature
	(12,1,	N'Sales Unit',		N'التسويق والمبيعات',		N'12',	N'SellingGeneralAndAdministration'),-- only expense by nature
	(13,1,	N'Project Mgmt',	N'إدارة المشاريع',			N'13',	N'SellingGeneralAndAdministration'), -- like comp. dep. of Abu Bakr to be absorbed by DL
	(19,1,	N'Services Unit',	N'وحدة الخدمات',			N'19',	N'SharedExpenseControl'), -- Rent, Power, and IT support

	(2,0, N'IT Solutions',		N'حلول تقنية',				N'2',	N'Abstract'),
	-- Babylon projects cross year boundaries
	(21,2,N'B10/HCM',			N'بابل',					N'21',	N'BusinessUnit'),
	(210,21,N'Campus BU',		N'بابل: وحدة الأعمال',		N'2100',	N'CostOfSales'), -- only expense by nature
	(211,21,N'B10/HCM  Projects',N'مشاريع بابل',			N'211',	N'Abstract'), 
	(2111,2,N'B10/HCM - IUA',	N'بابل - IUA',				N'2111',	N'ProductionExpenseControl'),
	
	-- For bsmart, we don't have phases. Every job is actually a separate client. Job expenditures are expensed immediately
	(2200,2,N'BSmart',			N'بيسمارت',					N'2200',	N'BusinessUnit'),
	
	-- For campus, some jobs may cross year boundaries. 
	(23,2,N'Campus',			N'كامبوس',					N'23',	N'BusinessUnit'),
	(230,23,N'Campus BU',		N'كامبوس: وحدة الأعمال',	N'2300',N'CostOfSales'), -- only expense by nature
	(231,23,N'Campus Projects',	N'مشاريع كامبوس',			N'231',	N'Abstract'), 
	(2311,231,N'Hayat University',N'جامعة الحياة',			N'2311',N'ProductionExpenseControl'), -- only expense by nature

	-- For Tellma, we have jobs that may cross years boundaries.
	(24,2,N'Tellma',			N'تلما',					N'24',	N'BusinessUnit'),
	(2400,24,N'Tellma BU',		N'تلما: وحدة الأعمال',		N'2400',N'CostOfSales'), -- everything
	(241,24,N'Tellma WIP',		N'مشاريع تلما قيد التنفيذ',N'241',	N'Abstract'),
	(2411,241,N'SSIA ',			N'جهاز الاستثمار',			N'2411',N'ProductionExpenseControl'),-- only expense by nature
	(2412,241,N'Cement Company',N'شركة الإسمنت',				N'2412',N'ProductionExpenseControl'),-- only expense by nature

	(25,2,N'Joint',				N'مشترك',					N'25',	N'BusinessUnit'),
	(2500,25,N'Joint ventures',	N'وحدة أعمال مشتركة',		N'2500',N'CostOfSales'), -- everything
	(251,3,	N'JV WIP',			N'مشاريع مشتركة قيد التنفيذ',N'251',N'Abstract'),
	(2510,3,N'IUA Project',		N'مشروع جامعة أفريقيا',	N'251',	N'CostOfSales'), -- everything

	(2900,2,N'Misc. IT',		N'حلول تقنية أخرى',		N'2900',	N'BusinessUnit'),

	(299,2,	N'Project - TBA',	N'المشاريع - للتخصيص',		N'299',	N'SharedExpenseControl'), -- like comp. dep. of Abu Bakr to be absorbed by DL
	(3,0,	N'Subletting',		N'تأجير',					N'3',	N'BusinessUnit')

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DECLARE @101CHQ INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Headquarters');
DECLARE @101CEO INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Exec. Office');
DECLARE @101CSales INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Sales Unit');
DECLARE @101CServices INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Services Unit');
DECLARE @101CB10 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'B10/HCM');
DECLARE @101CBSmart INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'BSmart');
DECLARE @101CCampus INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Campus');
DECLARE @101CTellma INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Tellma');
DECLARE @101MiscIT INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Misc. IT');