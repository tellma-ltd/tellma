	DECLARE @Creditors dbo.[ContractList];

IF @DB = N'100' -- ACME, USD, en/ar/zh		
	INSERT INTO @Creditors
	([Index],	[Name]) VALUES
	(0,			N'Creditor1'),
	(1,			N'Creditor2'),
	(2,			N'Creditor3'),
	(3,			N'Creditor4');
ELSE IF @DB = N'101' -- Banan SD, USD, en
	Print N'Tellma.' + @DB;
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	Print N'Tellma.' + @DB;

ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	Print N'Tellma.' + @DB

ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	Print N'Tellma.' + @DB

ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	Print N'Tellma.' + @DB

	EXEC [api].[Contracts__Save]
		@DefinitionId = @creditorsDef,
		@Entities = @Creditors,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Creditors: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;