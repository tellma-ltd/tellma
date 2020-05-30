	DECLARE @Custodians dbo.[ContractList];

IF @DB = N'100' -- ACME, USD, en/ar/zh
	Print N''
ELSE IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @Custodians
	([Index],	[Name],						[Name2],					[UserId]) VALUES
	(0,			N'GM Safe',					N'خزنة المدير العام',		@amtaam),
--	(1,			N'GM Safe - USD',			N'خزنة المدير العام - دولار',	@amtaam),
	(2,			N'Ahmad Abdussalam - Cash', N'أحمد عبد السلام - نقدية',	@aasalam),
	(3,			N'Bank of Khartoum',		N'بنك الخرطوم',				@amtaam),
	(4,			N'Admin Petty Cash',		N'النثرية الإدارية',		@Omer)
	;
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	INSERT INTO @Custodians
	([Index], [Name]) VALUES
	(0,		N'Mohamad Akra'),
	(1,		N'Wondewsen Semaneh'),
	(2,		N'Loay Bayazid'),
	(3,		N'Abu Bakr al-Hadi')
	;
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	Print N''
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @Custodians
	([Index], [Name]) VALUES
	(0,		N'Cashier');
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	Print N''

	EXEC [api].[Contracts__Save]
		@DefinitionId = @cash_accountsCD,
		@Entities = @Custodians,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'custodies: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DECLARE @GMSafe INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'GM Safe' AND [DefinitionId] = @cash_accountsCD);
--	DECLARE @GMSafeUSD INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'GM Safe - USD' AND [DefinitionId] = @cash_accountsCD);
	DECLARE @AdminPettyCash INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Admin Petty Cash' AND [DefinitionId] =  @cash_accountsCD);
	DECLARE @KSASafe INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Ahmad Abdussalam - Cash' AND [DefinitionId] =  @cash_accountsCD);
	DECLARE @KRTBank INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Bank of Khartoum' AND [DefinitionId] =  @cash_accountsCD);
