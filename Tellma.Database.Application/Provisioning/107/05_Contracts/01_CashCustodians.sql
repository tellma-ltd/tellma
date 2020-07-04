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

DELETE FROM @BankContracts;
DELETE FROM @ContractUsers;
INSERT INTO @BankContracts([Index],
	[Name],							
	[Name2],												[Code], [BankAccountNumber], [CurrencyId], [CenterId]) VALUES
(0,	N'Omdurman - Fetihab - 337226',
	N'بنك أمدرمان الوطني - فرع الفتيحاب - ح رقم 337226',	N'1001', N'337226',			@SDG,			@107C_SSIA);
INSERT INTO @ContractUsers([Index], [HeaderIndex], [UserId]) VALUES
(0,0,@AdminUserId)

EXEC [api].[Contracts__Save]
	@DefinitionId = @BankAccountCD,
	@Entities = @BankContracts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Cash on hand contracts: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;