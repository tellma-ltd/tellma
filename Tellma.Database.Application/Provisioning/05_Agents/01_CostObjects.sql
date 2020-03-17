	DECLARE @CostObjects dbo.[AgentList];

IF @DB = N'100' -- ACME, USD, en/ar/zh		
	INSERT INTO @CostObjects
	([Index],	[Name]) VALUES
	(0,			N'Cost Object 1'),
	(1,			N'Cost Object 2'),
	(2,			N'Cost Object 3'),
	(3,			N'Cost Object 4');
ELSE IF @DB = N'101' -- Banan SD, USD, en
-- TODO: Add IsCostEntity to table Agents
BEGIN
	INSERT INTO @CostObjects
	([Index],	[Name],			[Name2]) VALUES
	(0,			N'Babylon/HCM', N'بابل'),
	(1,			N'BSmart',		N'بيسمارت'),
	(2,			N'Tellma',		N'تلما'),
	(9,			N'Overhead',	N'غير مباشر');
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	Print N'Tellma.' + @DB;

ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	Print N'Tellma.' + @DB

ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	Print N'Tellma.' + @DB

ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	Print N'Tellma.' + @DB

	EXEC [api].[Agents__Save]
		@DefinitionId = N'cost-objects',
		@Entities = @CostObjects,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'CostEntities: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
	DECLARE @1Babylon INT = (SELECT [Id] FROM dbo.Agents WHERE [Name] = N'Babylon/HCM');
	DECLARE @1BSmart INT = (SELECT [Id] FROM dbo.Agents WHERE [Name] = N'BSmart');
	DECLARE @1Tellma INT = (SELECT [Id] FROM dbo.Agents WHERE [Name] = N'Tellma');
	DECLARE @1Overhead INT = (SELECT [Id] FROM dbo.Agents WHERE [Name] = N'Overhead');