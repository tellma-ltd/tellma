-- Safe
DELETE FROM @IndexedIds
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Relations] WHERE DefinitionId = @SafeCD;
EXEC [api].[Relations__Delete]
	@DefinitionId = @SafeCD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Safes: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @SafeCustodies; DELETE FROM @RelationUsers;
INSERT INTO @SafeCustodies([Index],	
	[Code], [Name],		[Name2],				[CenterId], [CurrencyId]) VALUES
(0,	N'CA0',	N'GM Safe',	N'خزنة المدير العام',	@101C1,		NULL);
INSERT INTO @RelationUsers([Index], [HeaderIndex], 
	[UserId]) VALUES
(0,0,@amtaam)
EXEC [api].[Relations__Save]
	@DefinitionId = @SafeCD,
	@Entities = @SafeCustodies,
	@RelationUsers = @RelationUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Safe Custodies: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DECLARE @GMSafe INT = (SELECT [Id] FROM dbo.[Relations] WHERE [Name] = N'GMSafe' AND [DefinitionId] =  @SafeCD);

-- Bank Account
DELETE FROM @IndexedIds
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Relations] WHERE DefinitionId = @BankAccountCD;
EXEC [api].[Relations__Delete]
	@DefinitionId = @BankAccountCD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Bank accounts: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @BankAccountCustodies; DELETE FROM @RelationUsers;
INSERT INTO @BankAccountCustodies([Index],	
	[Code], [Name],					[Name2],		[CenterId], [CurrencyId]) VALUES
(0,	N'B0',	N'Bank of Khartoum',	N'بنك الخرطوم',@101C1,		N'SDG');
INSERT INTO @RelationUsers([Index], [HeaderIndex], 
	[UserId]) VALUES
(0,0,@amtaam)
EXEC [api].[Relations__Save]
	@DefinitionId = @BankAccountCD,
	@Entities = @BankAccountCustodies,
	@RelationUsers = @RelationUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Bank Accounts Custodies: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DECLARE @KRTBank INT = (SELECT [Id] FROM dbo.[Relations] WHERE [Name] = N'Bank of Khartoum' AND [DefinitionId] =  @BankAccountCD);

-- Customer
DELETE FROM @IndexedIds
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Relations] WHERE DefinitionId = @CustomerCD;
EXEC [api].[Relations__Delete]
	@DefinitionId = @CustomerCD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Customers: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @Relations; DELETE FROM @RelationUsers;
INSERT INTO @Relations([Index],	
	[Code], [Name],								[Name2],					[CenterId], [CurrencyId]) VALUES
(0,	N'C01',N'International African University', N'جامعة أفريقيا العالمية',	NULL,		N'USD'),
(1,	N'C02',N'Mico poultry',						N'ميكو',					@101CB10,	N'USD'),
(2,	N'C03',N'Sabco',							N'سابكو',					@101CB10,	N'USD'),
(3,	N'C04',N'al-Washm',							N'شركة الوشم',				@101CB10,	N'SAR'),
(4,	N'C05',N'TAGI restaurants',					N'مطاعم تاجي',				@101CB10,	N'SAR'),
(5,	N'C06',N'It3aam',							N'شركة إطعام',				@101MiscIT,	N'USD'),
(6,	N'C07',N'Rafeef',							N'شركة رفيف',				@101CBSmart,N'SDG'),
(7,	N'C08',N'Golden Earth',						N'غولدن إيرث',				@101CBSmart,N'USD');
EXEC [api].[Relations__Save]
	@DefinitionId = @CustomerCD,
	@Entities = @Relations,
	@RelationUsers = @RelationUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Customer: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DECLARE @It3am INT = (SELECT [Id] FROM [dbo].[fi_Relations](N'customers', NULL) WHERE [Name] = N'It3aam');
DECLARE @Washm INT = (SELECT [Id] FROM [dbo].[fi_Relations](N'customers', NULL) WHERE [Name] = N'al-Washm');
DECLARE @Taji INT = (SELECT [Id] FROM [dbo].[fi_Relations](N'customers', NULL) WHERE [Name] = N'TAGI restaurants');
-- Partners
DELETE FROM @IndexedIds
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Relations] WHERE DefinitionId = @PartnerCD;
EXEC [api].[Relations__Delete]
	@DefinitionId = @PartnerCD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Partners: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @Relations; DELETE FROM @RelationUsers;
INSERT INTO @Relations([Index],	
	[Code],	[Name],				[Name2]) VALUES
(0,	N'P1',	N'Mohamad Akra',	N'محمد عكره'),
(1,	N'P2',	N'elAmin alTayeb',	N'الأمين الطيب'),
(2,	N'P3',	N'Abdullah Ulber',	N'عبد الله ألبر');

EXEC [api].[Relations__Save]
	@DefinitionId = @PartnerCD,
	@Entities = @Relations,
	@RelationUsers = @RelationUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'partners: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- Suppliers
DELETE FROM @IndexedIds
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Relations] WHERE DefinitionId = @SupplierCD;
EXEC [api].[Relations__Delete]
	@DefinitionId = @SupplierCD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Suppliers: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @Relations; DELETE FROM @RelationUsers;
INSERT INTO @Relations([Index],	
	[Code],	[Name],					[Name2]) VALUES
(0,	N'S01',	N'Tellma',				N'تلما'),
(1,	N'S02',	N'Salanco',				N'سلانكو'),
(2,	N'S03',	N'Canar',				N'كنار'),
(3,	N'S04',	N'Car Rental',			N'شركة تأجير السيارات'),
(4,	N'S05',	N'The Family Shawerma', N'شاورما العائلة'),
(99,N'S99',	N'Generic Supplier',	N'مورد عام');
EXEC [api].[Relations__Save]
	@DefinitionId = @SupplierCD,
	@Entities = @Relations,
	@RelationUsers = @RelationUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Suppliers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- Employees
DELETE FROM @IndexedIds
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Relations] WHERE DefinitionId = @EmployeeCD;
EXEC [api].[Relations__Delete]
	@DefinitionId = @EmployeeCD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Employees: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @Relations; DELETE FROM @RelationUsers;
INSERT INTO @Relations([Index],	
	[Code],	[Name],				[Name2], [CenterId]) VALUES
(0,	N'E001',N'Ahmad Habashi',	N'أحمد حبشي', @101C1),
(1,	N'E002',N'Ahmad Abdussalam',N'أحمد عبدالسلام', @101CB10),
(2,	N'E003',N'Abu Ammar',		N'أبو عمار', @101C1),
(3,	N'E004',N'Mohamad Ali',		N'محمد علي', @101C1),
(4,	N'E005',N'elAmin elTayeb',	N'الأمين الطيب', @101C1),
(5,	N'E099',N'M. Kamil',		N'محمد كامل', @101CB10)
EXEC [api].[Relations__Save]
	@DefinitionId = @EmployeeCD,
	@Entities = @Relations,
	@RelationUsers = @RelationUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Employees: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DECLARE @Abu_Ammar INT, @M_Ali INT, @el_Amin INT;
DECLARE @MohamadAkra int, @AhmadAkra int, @MKamil INT, @AASalamEmp INT;
SELECT
	@Abu_Ammar = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Abu Ammar'), 
	@M_Ali = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Mohamad Ali'), 
	@el_Amin = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'elAmin elTayeb'), 
	@MKamil = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'M. Kamil'),
	@AASalamEmp = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Ahmad Abdussalam')