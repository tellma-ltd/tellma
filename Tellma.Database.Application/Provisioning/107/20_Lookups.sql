-- PaperType, temporary
SET @DefinitionId = @BankLKD; DELETE FROM @Lookups;
	INSERT INTO @Lookups([Index],
	[Name],						[Name2]) VALUES
	(0,	N'Omdurman Bank',		N'بنك أمدرمان'),
	(1,	N'Khartoum Bank',		N'بنك الخرطوم')	,
	(2,	N'Salam Bank',			N'بنك السلام')	,
	(3,	N'El-Nilien Bank',		N'بنك النيلين')	;
EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'PaperType Lookups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;