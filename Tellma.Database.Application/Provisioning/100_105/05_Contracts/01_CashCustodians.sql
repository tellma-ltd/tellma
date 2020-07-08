	DECLARE @cashiers dbo.[ContractList];
	DECLARE @petty_cash_funds dbo.[ContractList];
	DECLARE @bank_accounts dbo.[ContractList];
	
-- Cashiers
DELETE FROM @ContractUsers;
IF @DB = N'100' -- ACME, USD, en/ar/zh
	Print N''

ELSE IF @DB = N'102' -- Banan ET, ETB, en
	Print N''
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	Print N''
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @cashiers
	([Index], [Name]) VALUES
	(0,		N'Cashier - Tigist');
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	Print N''
EXEC [api].[Contracts__Save]
	@DefinitionId = @CashOnHandAccountCD,
	@Entities = @cashiers,
	@ContractUsers = @ContractUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Cashiers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
	-- Petty Cash Funds
IF @DB = N'100' -- ACME, USD, en/ar/zh
	Print N''
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	INSERT INTO @petty_cash_funds
	([Index], [Name]) VALUES
	(0,		N'Mohamad Akra'),
	(1,		N'Wondewsen Semaneh'),
	(2,		N'Loay Bayazid'),
	(3,		N'Abu Bakr al-Hadi')
	;
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	Print N''
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @petty_cash_funds
	([Index], [Name]) VALUES
	(0,		N'Cashier');
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	Print N''
EXEC [api].[Contracts__Save]
	@DefinitionId = @CashOnHandAccountCD,
	@Entities = @petty_cash_funds,
	@ContractUsers = @ContractUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Petty Cash Funds: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- Bank Accounts
DELETE FROM @ContractUsers;
IF @DB = N'100' -- ACME, USD, en/ar/zh
	Print N''
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	Print N''
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	Print N''
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	Print N''
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	Print N''
EXEC [api].[Contracts__Save]
	@DefinitionId = @BankAccountCD,
	@Entities = @bank_accounts,
	@ContractUsers = @ContractUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Bank Accounts: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;


