	DECLARE @cashiers dbo.[ContractList];
	DECLARE @petty_cash_funds dbo.[ContractList];
	DECLARE @bank_accounts dbo.[ContractList];

	-- Cashiers
IF @DB = N'100' -- ACME, USD, en/ar/zh
	Print N''
ELSE IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @cashiers
	([Index],	[Name],
	[Name2],					[UserId]) VALUES
	(0,			N'GM Safe',					N'خزنة المدير العام',		@amtaam);
	;
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
	@DefinitionId = @cashiersCD,
	@Entities = @cashiers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Cashiers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
	-- Petty Cash Funds
IF @DB = N'100' -- ACME, USD, en/ar/zh
	Print N''
ELSE IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @petty_cash_funds
	([Index],	[Name],
	[Name2],					[UserId]) VALUES
	(2,			N'Ahmad Abdussalam - Cash', N'أحمد عبد السلام - نقدية',	@aasalam),
	(4,			N'Admin Petty Cash',		N'النثرية الإدارية',		@Omer)
	;
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
	@DefinitionId = @petty_cash_fundsCD,
	@Entities = @petty_cash_funds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Petty Cash Funds: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
	-- Bank Accounts
IF @DB = N'100' -- ACME, USD, en/ar/zh
	Print N''
ELSE IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @bank_accounts
	([Index],	[Name],
	[Name2],					[UserId]) VALUES
	(3,			N'Bank of Khartoum',		N'بنك الخرطوم',				@amtaam)
	;
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	Print N''
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	Print N''
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	Print N''
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	Print N''
EXEC [api].[Contracts__Save]
	@DefinitionId = @bank_accountsCD,
	@Entities = @bank_accounts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Bank Accounts: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;


	DECLARE @GMSafe INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'GM Safe' AND [DefinitionId] = @cashiersCD);
--	DECLARE @GMSafeUSD INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'GM Safe - USD' AND [DefinitionId] = @cash_accountsCD);
	DECLARE @AdminPettyCash INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Admin Petty Cash' AND [DefinitionId] =  @petty_cash_fundsCD);
	DECLARE @KSASafe INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Ahmad Abdussalam - Cash' AND [DefinitionId] =  @petty_cash_fundsCD);
	DECLARE @KRTBank INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Bank of Khartoum' AND [DefinitionId] =  @bank_accountsCD);