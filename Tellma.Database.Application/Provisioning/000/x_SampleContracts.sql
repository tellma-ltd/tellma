DECLARE @Agent1 INT;
DECLARE @CashOnHandAccount1 INT, @BankAccount1 INT;
DECLARE @Supplier1 INT, @Customer1 INT, @Employee1 INT, @Warehouse1 INT, @Creditor1 INT, @Debtor1 INT, @Partner1 INT;

DECLARE @Agent2 INT;
DECLARE @CashOnHandAccount2 INT, @BankAccount2 INT;
DECLARE @Supplier2 INT, @Customer2 INT, @Employee2 INT, @Warehouse2 INT, @Creditor2 INT, @Debtor2 INT, @Partner2 INT;

DECLARE @Agent3 INT;
DECLARE @CashOnHandAccount3 INT, @BankAccount3 INT;
DECLARE @Supplier3 INT, @Customer3 INT, @Employee3 INT, @Warehouse3 INT, @Creditor3 INT, @Debtor3 INT, @Partner3 INT;

DELETE FROM @Agents; DELETE FROM @Contracts; DELETE FROM @ContractUsers;
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
DELETE FROM @Contracts
INSERT INTO @Contracts(
	[Index],	[Code],		[Name],				[CurrencyId],			[CenterId],	[LocationJson],	[FromDate],	[ToDate],	[Decimal1],	[Decimal2],			
	[Int1],		[Int2],		[Lookup1Id],		[Lookup2Id],			[Lookup3Id],[Lookup4Id],	[Text1],	[Text2],	[AgentId],	[TaxIdentificationNumber],
	[JobId],	[BankAccountNumber]) VALUES
(	0,			N'CA1',		N'Cash Account 1',	@FunctionalCurrencyId,	NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		@Agent1,	NULL,
	NULL,		NULL),
(	1,			N'CA2',		N'Cash Account 2',	NUll,					NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		NULL,	NULL,
	NULL,		NULL),
(	2,			N'CA3',		N'Cash Account 3',	NUll,					NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		NULL,	NULL,
	NULL,		NULL);

EXEC [api].[Contracts__Save]
	@DefinitionId = @CashOnHandAccountCD,
	@ContractUsers = @ContractUsers,
	@Entities = @Contracts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;


IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Cash Accounts: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
SELECT @CashOnHandAccount1 = [Id] FROM dbo.Contracts WHERE [Code] = N'CA1';
SELECT @CashOnHandAccount2 = [Id] FROM dbo.Contracts WHERE [Code] = N'CA2';
SELECT @CashOnHandAccount3 = [Id] FROM dbo.Contracts WHERE [Code] = N'CA3';

-- Adding sample bank accounts
DELETE FROM @Contracts -- Text1: Branch, Lookup1: Bank Account type
INSERT INTO @Contracts(
	[Index],	[Code],		[Name],				[CurrencyId],			[CenterId],	[LocationJson],	[FromDate],	[ToDate],	[Decimal1],	[Decimal2],			
	[Int1],		[Int2],		[Lookup1Id],		[Lookup2Id],			[Lookup3Id],[Lookup4Id],	[Text1],	[Text2],	[AgentId],	[TaxIdentificationNumber],
	[JobId],	[BankAccountNumber]) VALUES
(	0,			N'B01',		N'Bank Account 1',	@FunctionalCurrencyId,	NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			N'Area 1',	NULL,		NULL,		NULL,
	NULL,		N'1111'),
(	1,			N'B02',		N'Bank Account 2',	NUll,					NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			N'Area 2',	NULL,		NULL,		NULL,
	NULL,		N'2222'),
(	2,			N'B03',		N'Bank Account 3',	NUll,					NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		N'3333');

EXEC [api].[Contracts__Save]
	@DefinitionId = @BankAccountCD,
	@ContractUsers = @ContractUsers,
	@Entities = @Contracts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;


IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Bank Accounts: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

SELECT @BankAccount1 = [Id] FROM dbo.Contracts WHERE [Code] = N'B01';
SELECT @BankAccount2 = [Id] FROM dbo.Contracts WHERE [Code] = N'B02';
SELECT @BankAccount3 = [Id] FROM dbo.Contracts WHERE [Code] = N'B03';
-- Adding sample Supplier accounts
DELETE FROM @Contracts -- 
INSERT INTO @Contracts( -- text1: email, text2: phone
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
EXEC [api].[Contracts__Save]
	@DefinitionId = @SupplierCD,
	@ContractUsers = @ContractUsers,
	@Entities = @Contracts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Suppliers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Adding sample Customer accounts
DELETE FROM @Contracts -- 
INSERT INTO @Contracts( -- text1: email, text2: phone, lookup1: MarketSegment
	[Index],	[Code],		[Name],				[CurrencyId],			[CenterId],	[LocationJson],	[FromDate],	[ToDate],	[Decimal1],	[Decimal2],			
	[Int1],		[Int2],		[Lookup1Id],		[Lookup2Id],			[Lookup3Id],[Lookup4Id],	[Text1],	[Text2],	[AgentId],	[TaxIdentificationNumber],
	[JobId],	[BankAccountNumber]) VALUES
(	0,			N'C0001',	N'Customer 1',		@FunctionalCurrencyId,	NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		@Agent1,	NULL,
	NULL,		NULL),
(	1,			N'C0002',	N'Customer 2',		NUll,					NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		NULL,		N'TX-22',
	NULL,		NULL),
(	2,			N'C0003',	N'Customer 3',		NUll,					NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		@Agent3,	NULL,
	NULL,		NULL);
EXEC [api].[Contracts__Save]
	@DefinitionId = @CustomerCD,
	@ContractUsers = @ContractUsers,
	@Entities = @Contracts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Customers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Adding sample Employee accounts
DELETE FROM @Contracts -- 
INSERT INTO @Contracts( -- text1: email, text2: phone, Gender:, Color, Marital Status, Date Of Birth, 
	[Index],	[Code],		[Name],				[CurrencyId],			[CenterId],	[LocationJson],	[FromDate],	[ToDate],	[Decimal1],	[Decimal2],			
	[Int1],		[Int2],		[Lookup1Id],		[Lookup2Id],			[Lookup3Id],[Lookup4Id],	[Text1],	[Text2],	[AgentId],	[TaxIdentificationNumber],
	[JobId],	[BankAccountNumber]) VALUES
(	0,			N'E001',	N'Employee 1',		@FunctionalCurrencyId,	NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		@Agent1,	NULL,
	NULL,		NULL),
(	1,			N'E002',	N'Employee 2',		NUll,					NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		NULL,		N'TX-22',
	NULL,		NULL),
(	2,			N'E003',	N'Employee 3',		NUll,					NULL,		NULL,			NULL,		NULL,		NULL,		NULL,
	NULL,		NULL,		NULL,				NULL,					NULL,		NULL,			NULL,		NULL,		@Agent3,	NULL,
	NULL,		NULL);
EXEC [api].[Contracts__Save]
	@DefinitionId = @EmployeeCD,
	@ContractUsers = @ContractUsers,
	@Entities = @Contracts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Employees: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;