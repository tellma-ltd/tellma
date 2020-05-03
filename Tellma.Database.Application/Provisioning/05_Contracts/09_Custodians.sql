	DECLARE @Custodians dbo.[ContractList];

IF @DB = N'100' -- ACME, USD, en/ar/zh
	Print N''
ELSE IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @Custodians
	([Index],	[Name],						[Name2],					[UserId]) VALUES
	(0,			N'elAmin Attayyib - Cash',	N'الأمين الطيب - نقدية',	@amtaam),
	(1,			N'Ahmad Abdussalam - Cash', N'أحمد عبد السلام - نقدية',	@aasalam),
	(2,			N'Bank of Khartoum',		N'بنك الخرطوم',				@amtaam),
	(3,			N'Omar el-Samani - Cash',	N'عمر السماني - نقدية',	@Omer)
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
		@DefinitionId = @cash_custodiansDef,
		@Entities = @Custodians,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'custodies: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DECLARE @GMSafe INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'elAmin Attayyib - Cash' AND [DefinitionId] = @cash_custodiansDef);
	DECLARE @AdminSafe INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Omar el-Samani - Cash' AND [DefinitionId] =  @cash_custodiansDef);
	DECLARE @KSASafe INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Ahmad Abdussalam - Cash' AND [DefinitionId] =  @cash_custodiansDef);
	DECLARE @KRTBank INT = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Bank of Khartoum' AND [DefinitionId] =  @cash_custodiansDef);