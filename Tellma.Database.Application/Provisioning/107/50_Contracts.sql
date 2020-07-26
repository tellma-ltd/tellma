

DELETE FROM @Agents; DELETE FROM @Relations; DELETE FROM @RelationUsers;
-- Adding sample agents
INSERT INTO @Agents([Index], [Name], [IsRelated]) VALUES
(0,N'Agent 1', 0),
(1,N'Agent 2', 1),
(2,N'Agent 3', 0);

EXEC [api].[Agents__Save]
	@Entities = @Agents,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Agents: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

SELECT @Agent1 = [Id] FROM dbo.Agents WHERE [Name] = N'Agent 1'; -- employee 1 and cash custodian 1 and customer 1
SELECT @Agent2 = [Id] FROM dbo.Agents WHERE [Name] = N'Agent 2'; -- employee 2 and partner 2 (related)
SELECT @Agent3 = [Id] FROM dbo.Agents WHERE [Name] = N'Agent 3'; -- customer 3 and supplier 3

-- Adding sample cash accounts
DELETE FROM @Relations
INSERT INTO @Relations(
	[Index],	[Code],		[Name],					[CurrencyId],		[CenterId],			[LocationJson],	[FromDate],		[ToDate],	[Decimal1],	[Decimal2],			
	[Int1],		[Int2],		[Name2],				[Lookup1Id],		[Lookup2Id],		[Lookup3Id],	[Lookup4Id],	[Text1],	[Text2],	[AgentId],	[TaxIdentificationNumber],
	[JobId],	[BankAccountNumber]) VALUES
(	0,			N'CA1',		N'Exec Off. Cash - SDG',N'SDG',				@107C_Headquarters,	NULL,			NULL,			NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,					NULL,				NULL,				NULL,			NULL,			NULL,		NULL,		@Agent1,	NULL,
	NULL,		NULL),
(	1,			N'CA2',		N'Purchase Funds - SDG',N'SDG',				@107C_Headquarters,	NULL,			NULL,			NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,					NULL,				NULL,				NULL,			NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL),
(	2,			N'CA3',		N'Travel Cash - USD',	N'USD',				@107C_Headquarters,	NULL,			NULL,			NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,					NULL,				NULL,				NULL,			NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL),
(	3,			N'CA4',		N'Cashier',				NULL,				@107C_MehiraScheme,	NULL,			NULL,			NULL,		NULL,		NULL,
	NULL,		NULL,		N'الصندوق',				NULL,				NULL,				NULL,			NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL)
	
	
	;

EXEC [api].[Relations__Save]
	@DefinitionId = @EmployeeCD,
	@RelationUsers = @RelationUsers,
	@Entities = @Relations,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;


IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Cash Accounts: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
SELECT @CashOnHandAccount1 = [Id] FROM dbo.[Relations] WHERE [Code] = N'CA1';
SELECT @CashOnHandAccount2 = [Id] FROM dbo.[Relations] WHERE [Code] = N'CA2';
SELECT @CashOnHandAccount3 = [Id] FROM dbo.[Relations] WHERE [Code] = N'CA3';
SELECT @CashOnHandAccount4 = [Id] FROM dbo.[Relations] WHERE [Code] = N'CA4';

-- Adding sample bank accounts
DELETE FROM @Relations -- Text1: Branch, Lookup1: Bank Account type
INSERT INTO @Relations(
	[Index],	[Code],		[Name],					[CurrencyId],	[CenterId],			[LocationJson],	[FromDate],	[ToDate],	[Decimal1],	[Decimal2],			
	[Int1],		[Int2],		[Name2],				[Lookup1Id],	[Lookup2Id],		[Lookup3Id],	[Lookup4Id],[Text1],	[Text2],	[AgentId],	[TaxIdentificationNumber],
	[JobId],	[BankAccountNumber]) VALUES
(	0,			N'B01',		N'Omdurman - SDG',		N'SDG',			@107C_Headquarters,	NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		N'أم درمان - جنيه',	NULL,			NULL,				NULL,			NULL,		N'الفتيحاب',NULL,		NULL,		NULL,
	NULL,		N'1111'),
(	1,			N'B02',		N'Omdurman - USD',		N'USD',			@107C_Headquarters,	NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		N'أم درمان - دولار',	NULL,			NULL,				NULL,			NULL,		N'الكلاكلا',	NULL,		NULL,		NULL,
	NULL,		N'2222'),
(	2,			N'B03',		N'Salam - Mehira - SDG',N'SDG',			@107C_MehiraScheme,	NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		N'السلام - مهيرة - حنيه',NULL,			NULL,				NULL,			NULL,		NULL,		NULL,		NULL,		NULL,
	NULL,		N'3333');

EXEC [api].[Relations__Save]
	@DefinitionId = @BankCD,
	@RelationUsers = @RelationUsers,
	@Entities = @Relations,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;


IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Bank Accounts: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

SELECT @BankAccount1 = [Id] FROM dbo.[Relations] WHERE [Code] = N'B01';
SELECT @BankAccount2 = [Id] FROM dbo.[Relations] WHERE [Code] = N'B02';
SELECT @BankAccount3 = [Id] FROM dbo.[Relations] WHERE [Code] = N'B03';
-- Adding sample Supplier accounts
DELETE FROM @Relations -- 
INSERT INTO @Relations( -- text1: email, text2: phone
	[Index],	[Code],		[Name],				[CurrencyId],			[CenterId],	[LocationJson],	[FromDate],	[ToDate],	[Decimal1],	[Decimal2],			
	[Int1],		[Int2],		[Lookup1Id],		[Lookup2Id],			[Lookup3Id],[Lookup4Id],	[Text1],	[Text2],	[AgentId],	[TaxIdentificationNumber],
	[JobId],	[BankAccountNumber]) VALUES
(	0,			N'S01',		N'Supplier 1',		@FunctionalCurrencyId,	NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		NULL,		N'TX-10-100',
	NULL,		NULL),
(	1,			N'S02',		N'Supplier 2',		NUll,					NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		NULL,		N'TX-20-200',
	NULL,		NULL),
(	2,			N'S03',		N'Supplier 3',		NUll,					NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		@Agent3,	N'TX-30-300',
	NULL,		NULL);
EXEC [api].[Relations__Save]
	@DefinitionId = @SupplierCD,
	@RelationUsers = @RelationUsers,
	@Entities = @Relations,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Suppliers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Adding sample Customer accounts
DELETE FROM @Relations -- 
INSERT INTO @Relations( -- text1: email, text2: phone, lookup1: MarketSegment
	[Index],	[Code],		[Name],				[CurrencyId],			[CenterId],			[LocationJson],	[FromDate],	[ToDate],	[Decimal1],	[Decimal2],			
	[Int1],		[Int2],		[Lookup1Id],		[Lookup2Id],			[Lookup3Id],		[Lookup4Id],	[Text1],	[Text2],	[AgentId],	[TaxIdentificationNumber],
	[JobId],	[BankAccountNumber]) VALUES
(	0,			N'C0001',	N'Customer 1',		N'SDG',					@107C_MehiraScheme,	NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,				NULL,			NULL,		NULL,		@Agent1,	NULL,
	NULL,		NULL),
(	1,			N'C0002',	N'Customer 2',		N'SDG',					@107C_MehiraScheme,	NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,				NULL,			NULL,		NULL,		NULL,		N'TX-22',
	NULL,		NULL),
(	2,			N'C0003',	N'Customer 3',		N'SDG',					@107C_MehiraScheme,	NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,				NULL,			NULL,		NULL,		@Agent3,	NULL,
	NULL,		NULL);
EXEC [api].[Relations__Save]
	@DefinitionId = @CustomerCD,
	@RelationUsers = @RelationUsers,
	@Entities = @Relations,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Customers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Adding sample Employee accounts
DELETE FROM @Relations -- 
INSERT INTO @Relations( -- text1: email, text2: phone, Gender:, Color, Marital Status, Date Of Birth, 
	[Index],	[Code],		[Name],				[CurrencyId],			[CenterId],				[LocationJson],	[FromDate],	[ToDate],	[Decimal1],	[Decimal2],			
	[Int1],		[Int2],		[Name2],			[Lookup1Id],			[Lookup2Id],			[Lookup3Id],	[Lookup4Id],[Text1],	[Text2],	[AgentId],	[TaxIdentificationNumber],
	[JobId],	[BankAccountNumber]) VALUES
(	0,			N'E001',	N'Ali',				@FunctionalCurrencyId,	@107C_MehiraScheme,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		N'علي',				NULL,					NULL,					NULL,			NULL,		NULL,		NULL,		@Agent1,	NULL,
	NULL,		NULL),
(	1,			N'E002',	N'Osman',			NUll,					@107C_MehiraScheme,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		N'عثمان',			NULL,					NULL,					NULL,			NULL,		NULL,		NULL,		NULL,		N'TX-22',
	NULL,		NULL),
(	2,			N'E003',	N'Employee 3',		NUll,					@107C_Headquarters,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		N'موظف 3',			NULL,					NULL,					NULL,			NULL,		NULL,		NULL,		@Agent3,	NULL,
	NULL,		NULL);
EXEC [api].[Relations__Save]
	@DefinitionId = @EmployeeCD,
	@RelationUsers = @RelationUsers,
	@Entities = @Relations,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Employees: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;