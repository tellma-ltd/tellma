	DELETE FROM @Centers;
	INSERT INTO @Centers([Index],[ParentIndex],
		[Name],					[Name2],					[Code],[CenterType]) VALUES
	(0,NULL,N'Banan',			N'بنان',					N'0',	N'Abstract'),
	(1,0,	N'Departments',		N'الإدارات',					N'1',	N'Abstract'),
	(110,1,	N'Exec. Office',	N'المكتب التنفيذي',		N'110',	N'SellingGeneralAndAdministration'),
	(120,1,	N'Sales Unit',		N'التسويق والمبيعات',		N'120',	N'SellingGeneralAndAdministration'),
	(130,1,	N'Services Unit',	N'وحدة الخدمات',			N'130',	N'SharedExpenseControl'), -- Rent, Power, and IT support
	(2,0,	N'Projects',		N'مشاريع',					N'2',	N'Abstract'),
	(20,2,	N'Project - O/H',	N'المشاريع - غ.م',			N'22',	N'SharedExpenseControl'), -- like comp. dep. of Avy Bakr to be absorbed by DL
	(22,2,	N'IUA',				N'جامعة أفريقيا',			N'22',	N'Abstract'),
	(221,22,N'IUA - Phase 1',	N'أفريقيا - 1',				N'221',	N'ProductionExpenseControl'),
	(222,22,N'IUA - Phase 2',	N'أفريقيا - 2',				N'222',	N'ProductionExpenseControl'),
	(23,2,	N'SSIA',			N'جهاز الاستثمار',			N'23',	N'Abstract'),
	(231,23,N'SSIA - Phase 1',	N'جهاز الاستثمار - 1',		N'231',	N'ProductionExpenseControl'),
	(232,23,N'SSIA - Phase 2',	N'جهاز الاستثمار - 2',		N'232',	N'ProductionExpenseControl'),
	(3,0,	N'Business Units',	N'وحدات أعمال',				N'3',	N'Abstract'),
	(31,3,	N'B10/HCM',			N'بابل',					N'31',	N'BusinessUnit'),
	(310,31,N'B10/HCM - COS',	N'بابل - م. مباشرة',		N'310',	N'CostOfSales'),
	(311,31,N'B10/HCM - SGA',	N'بابل - م. غير مباشرة',	N'311',N'SellingGeneralAndAdministration'),
	(32,3,	N'BSmart',			N'بيسمارت',					N'32',	N'BusinessUnit'),
	(320,32,N'BSmart - COS',	N'بيسمارت - م. مباشرة',	N'320',	N'CostOfSales'),
	(321,32,N'BSmart - SGA',	N'بيسمارت - م. غير مباشرة'	,N'321',N'SellingGeneralAndAdministration'),
	(33,3,	N'Campus',			N'كامبوس',					N'33',	N'BusinessUnit'),
	(330,33,N'Campus - COS',	N'كامبوس - م. مباشرة',		N'330',	N'CostOfSales'),
	(331,33,N'Campus - SGA',	N'كامبوس - م. غير مباشرة',	N'331',N'SellingGeneralAndAdministration'),
	(34,3,	N'Tellma',			N'تلما',					N'34',	N'BusinessUnit'),
	(340,34,N'Tellma - COS',	N'تلما - م. مباشرة',		N'340',	N'CostOfSales'),
	(341,34,N'Tellma - SGA',	'تلما - م. غير مباشرة',	N'341',N'SellingGeneralAndAdministration'),
	(35,3,	N'Consulting',		N'استشارات',				N'35',	N'BusinessUnit'),
	(350,35,N'Consulting - COS',N'استشارات - م. مباشرة',	N'350',	N'CostOfSales'),
	(351,35,N'Consulting - SGA',N'استشارات - م. غير مباشرة',N'351',N'SellingGeneralAndAdministration'),
	(39,3,	N'1st Floor',		N'ط - 1',					N'390',	N'BusinessUnit');

	-- There is already a center
	UPDATE @Centers SET [Id] = (SELECT MIN([Id]) FROM dbo.Centers)
	WHERE [Index] = 0

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DECLARE @101CBanan INT = (SELECT [Id] FROM dbp.Centers WHERE [Name] = N'Banan');
DECLARE @101CB10 INT = (SELECT [Id] FROM dbp.Centers WHERE [Name] = N'B10/HCM');
DECLARE @101CBSmart INT = (SELECT [Id] FROM dbp.Centers WHERE [Name] = N'BSmart');
DECLARE @101CCampus INT = (SELECT [Id] FROM dbp.Centers WHERE [Name] = N'Campus');
DECLARE @101CTellma INT = (SELECT [Id] FROM dbp.Centers WHERE [Name] = N'Tellma');
DECLARE @101CConsulting INT = (SELECT [Id] FROM dbp.Centers WHERE [Name] = N'Consulting');