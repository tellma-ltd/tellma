-- Safe
DELETE FROM @IndexedIds
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Custodies] WHERE DefinitionId = @SafeCD;
EXEC [api].[Custodies__Delete]
	@DefinitionId = @SafeCD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Safes: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @Safes;
INSERT INTO @Safes([Index],	
	[Code], [Name],		[Name2],				[CenterId], [CurrencyId]) VALUES
(0,	N'CA0',	N'GM Safe',	N'خزنة المدير العام',	@101C1,		NULL);
EXEC [api].[Custodies__Save]
	@DefinitionId = @SafeCD,
	@Entities = @Safes,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Safe Custodies: Inserting: ' + @ValidationErrorsJson
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
	[Code], [Name],					[Name2],		[CenterId], [CurrencyId]) VALUES
(0,	N'B0',	N'Bank of Khartoum',	N'بنك الخرطوم',@101C1,		N'SDG');

EXEC [api].[Custodies__Save]
	@DefinitionId = @BankAccountCD,
	@Entities = @BankAccountCustodies,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Bank Accounts Custodies: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
