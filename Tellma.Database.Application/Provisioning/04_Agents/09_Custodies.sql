	DECLARE @Custodies dbo.[AgentList];


IF @DB = N'100' -- ACME, USD, en/ar/zh
	Print N''
ELSE IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @Custodies
	([Index],	[Name]) VALUES
	(0,			N'GM Safe');
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	INSERT INTO @Custodies
	([Index], [Name]) VALUES
	(0,		N'GM Petty Cash');
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	Print N''
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @Custodies
	([Index], [Name]) VALUES
	(0,		N'Cashier');
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	Print N''

	EXEC [api].[Agents__Save]
		@DefinitionId = N'custodies',
		@Entities = @Custodies,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'custodies: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DECLARE @GMSafe INT = (SELECT [Id] FROM dbo.Agents WHERE Name = N'GM Safe');