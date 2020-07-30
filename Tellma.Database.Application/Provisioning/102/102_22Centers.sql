	DELETE FROM @Centers;
	INSERT INTO @Centers([Index],[ParentIndex],
		[Name],					[Code],[CenterType]) VALUES
	(0,NULL,N'Banan',			N'0',	N'Abstract'),
	
	(1,0,	N'Headquarters',	N'1',	N'BusinessUnit'),
	(11,1,	N'General & Admin',	N'11',	N'SellingGeneralAndAdministration'),-- only expense by nature
	(12,1,	N'Marketing',		N'12',	N'SellingGeneralAndAdministration'),-- only expense by nature


	(2,0,	N'Tellma Projects',	N'2',	N'BusinessUnit'),
	(20,1,	N'Projects O/H',	N'20',	N'SharedExpenseControl'), -- salaries, Cloud hosting
	(21,2,	N'Soreti ERP',		N'21',	N'CostOfSales'),
	(3,0,	N'Misc. Projects',	N'3',	N'BusinessUnit'),
	(31,3,	N'Lifan Support',	N'31',	N'CostOfSales')

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Centers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DECLARE @102C11 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Headquarters');
DECLARE @102C20 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Projects O/H');
DECLARE @102C21 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Soreti ERP');
DECLARE @102C31 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Lifan Support');