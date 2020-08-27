-- Cash On Hand
DELETE FROM @IndexedIds;
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Custodies] WHERE DefinitionId = @CashOnHandAccountCD;
EXEC [api].[Custodies__Delete]
	@DefinitionId = @CashOnHandAccountCD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Cash Accounts: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @CashOnHandAccounts;
INSERT INTO @CashOnHandAccounts([Index],	
	[Code], [Name],				[CenterId],			[CurrencyId]) VALUES
(0,	N'CA0',	N'GM Cash On Hand',	@106C_HeadOffice,	@ETB);

;
EXEC [api].[Custodies__Save]
	@DefinitionId = @CashOnHandAccountCD,
	@Entities = @CashOnHandAccounts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Cash Accounts: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Bank Account
DELETE FROM @IndexedIds
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Custodies] WHERE DefinitionId = @BankAccountCD;
EXEC [api].[Custodies__Delete]
	@DefinitionId = @BankAccountCD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Bank accounts: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @BankAccountCustodies;
INSERT INTO @BankAccountCustodies([Index],	
	[Code], [Name],				[CenterId],			[CurrencyId]) VALUES
(0,	N'B0',	N'CBE - USD',		@106C_HeadOffice,	@USD),
(1,	N'B1',	N'CBE - ETB',		@106C_HeadOffice,	@ETB),
(2,	N'B2',	N'AWB - ETB',		@106C_HeadOffice,	@ETB),
(3,	N'B3',	N'NIB - ETB',		@106C_HeadOffice,	@ETB);

EXEC [api].[Custodies__Save]
	@DefinitionId = @BankAccountCD,
	@Entities = @BankAccountCustodies,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Bank Accounts Custodies: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
