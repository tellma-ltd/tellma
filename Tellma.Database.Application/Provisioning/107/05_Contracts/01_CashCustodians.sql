DELETE FROM @CashOnHandContracts;
DELETE FROM @ContractUsers;
INSERT INTO @CashOnHandContracts([Index], [Name], [Name2]) VALUES
(0,			N'Cash 1',	NULL);
INSERT INTO @ContractUsers([Index], [HeaderIndex], [UserId]) VALUES
(0,0,@AdminUserId)

EXEC [api].[Contracts__Save]
	@DefinitionId = @CashOnHandAccountCD,
	@Entities = @CashOnHandContracts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Cash on hand contracts: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;