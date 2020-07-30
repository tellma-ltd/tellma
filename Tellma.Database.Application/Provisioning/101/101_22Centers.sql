	DELETE FROM @Centers;
	INSERT INTO @Centers([Index],[ParentIndex],
		[Name],					[Name2],					[Code],[CenterType]) VALUES
	(0,NULL,N'Banan',			N'بنان',					N'0',	N'Abstract'),
	
	(1,0,	N'Headquarters',	N'الرئاسة',					N'1',	N'BusinessUnit'),
	(11,1,	N'Exec. Office',	N'المكتب التنفيذي',		N'11',	N'SellingGeneralAndAdministration'),-- only expense by nature
	(12,1,	N'Sales Unit',		N'التسويق والمبيعات',		N'12',	N'SellingGeneralAndAdministration'),-- only expense by nature
	(13,1,	N'Project Mgmt',	N'إدارة المشاريع',			N'13',	N'SellingGeneralAndAdministration'), -- like comp. dep. of Abu Bakr to be absorbed by DL
	(19,1,	N'Services Unit',	N'وحدة الخدمات',			N'19',	N'SharedExpenseControl'), -- Rent, Power, and IT support

	(2,0, N'IT Solutions',		N'حلول تقنية',				N'2',	N'BusinessUnit'),
	(21,2,N'B10/HCM',			N'بابل',					N'21',	N'CostOfSales'),
	(22,2,N'BSmart',			N'بيسمارت',					N'22',	N'CostOfSales'),
	(23,2,N'Campus',			N'كامبوس',					N'23',	N'CostOfSales'),
	(24,2,N'Tellma',			N'تلما',					N'24',	N'CostOfSales'),
	(29,2,N'Misc. IT',			N'حلول تقنية أخرى',		N'29',	N'CostOfSales'),
	(3,0,N'Other Business',		N'أعمال أخرى',				N'3',	N'BusinessUnit'),
	(31,3,N'Subletting',		N'تأجير',					N'31',	N'CostOfSales');

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DECLARE @101C1 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Headquarters');
DECLARE @101C11 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Exec. Office');
DECLARE @101C12 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Sales Unit');
DECLARE @101C19 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Services Unit');
DECLARE @101CB10 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'B10/HCM');
DECLARE @101CBSmart INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'BSmart');
DECLARE @101CCampus INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Campus');
DECLARE @101CTellma INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Tellma');
DECLARE @101MiscIT INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Misc. IT');