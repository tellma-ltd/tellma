	INSERT INTO @CashOnHandContracts
	([Index],	[Name],		[Name2],	[UserId]) VALUES
	(0,			N'Cash 1',	NULL,		@AdminUserId)
	;

EXEC [api].[Contracts__Save]
	@DefinitionId = @CashOnHandAccountCD,
	@Entities = @cashiers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Cash on hand contracts: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;