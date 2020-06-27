DELETE FROM @CashOnHandContracts;
DELETE FROM @ContractUsers;
INSERT INTO @CashOnHandContracts([Index], [Name], [Name2], [CenterId]) VALUES
(0,			N'Cash 1',	NULL, @106C_TradingHO);
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

DELETE FROM @CashOnHandContracts;
DELETE FROM @ContractUsers;
INSERT INTO @CashOnHandContracts([Index], [Name], [Name2], [CenterId], [CurrencyId]) VALUES
(0,			N'Bank 1',	NULL, @106C_RealEstate, @ETB),
(1,			N'Bank 2',	NULL, @106C_TradingHO, @USD);
INSERT INTO @ContractUsers([Index], [HeaderIndex], [UserId]) VALUES
(0,0,@AdminUserId),
(0,1,@AdminUserId)

EXEC [api].[Contracts__Save]
	@DefinitionId = @BankAccountCD,
	@Entities = @CashOnHandContracts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Cash on hand contracts: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;