	DELETE FROM @Centers;
	INSERT INTO @Centers([Index],[ParentIndex],
			[Name],					[Code],[CenterType]) VALUES
	(1,NULL,N'Banan',			N'1',	N'Abstract'),
	
	(11,1,	N'Headquarters',	N'11',	N'BusinessUnit'),
	(111,11,N'General & Admin',	N'111',	N'SellingGeneralAndAdministration'),
	(112,11,N'Marketing',		N'112',	N'SellingGeneralAndAdministration'),

	(12,1,	N'Tellma Projects',	N'12',	N'BusinessUnit'),
	(120,12,N'Projects O/H',	N'120',	N'SharedExpenseControl'), -- salaries
	(121,12,N'Soreti ERP',		N'121',	N'CostOfSales'),
	(13,1,	N'Misc. Projects',	N'13',	N'BusinessUnit'),
	(131,13,N'Lifan Support',	N'131',	N'CostOfSales'),

	(2,NULL,N'BIOSS',			N'2',	N'Abstract'),
	
	(21,2,	N'B/Headquarters',	N'21',	N'BusinessUnit'),
	(211,21,N'B/General & Admin',N'211',N'SellingGeneralAndAdministration'),
	(212,21,N'B/Marketing',		N'212',	N'SellingGeneralAndAdministration'),

	(22,2,	N'B/Tellma Projects',N'22',	N'BusinessUnit'),
	(220,22,N'B/Projects O/H',	N'220',	N'SharedExpenseControl'), -- salaries, Cloud hosting
	(221,22,N'B/Soreti ERP',	N'221',	N'CostOfSales'),
	(23,2,	N'B/Misc. Projects',N'23',	N'BusinessUnit'),
	(231,23,N'B/Lifan Support',	N'231',	N'CostOfSales');
	
INSERT INTO @ValidationErrors
EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@UserId = @AdminUserId;
	
IF EXISTS (SELECT [Key] FROM @ValidationErrors)
BEGIN
	Print 'Centers: Error Inserting'
	GOTO Err_Label;
END;

DECLARE @102C11 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Headquarters');
DECLARE @102C20 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Projects O/H');
DECLARE @102C21 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Soreti ERP');
DECLARE @102C31 INT = (SELECT [Id] FROM dbo.Centers WHERE [Name] = N'Lifan Support');