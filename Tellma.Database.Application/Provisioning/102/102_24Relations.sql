-- Customer
DELETE FROM @IndexedIds
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Relations] WHERE DefinitionId = @CustomerRLD;
EXEC [api].[Relations__Delete]
	@DefinitionId = @CustomerRLD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Customers: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @Relations; DELETE FROM @RelationUsers;
INSERT INTO @Relations([Index],	
	[Code], [Name],					[CenterId], [CurrencyId], TaxIdentificationNumber) VALUES
(0,	N'C01',N'Yangfan Motors, PLC',	@102C31,	@ETB,			N'0005308731'),
(1,	N'C02',N'Soreti Trading',		@102C21,	@ETB,			NULL);
EXEC [api].[Relations__Save]
	@DefinitionId = @CustomerRLD,
	@Entities = @Relations,
	@RelationUsers = @RelationUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Customer: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Partners
DELETE FROM @IndexedIds
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Relations] WHERE DefinitionId = @PartnerRLD;
EXEC [api].[Relations__Delete]
	@DefinitionId = @PartnerRLD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Partners: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @Relations; DELETE FROM @RelationUsers;
INSERT INTO @Relations([Index],	
	[Code],	[Name]	) VALUES
(0,	N'P1',	N'Mohamad Akra'),
(1,	N'P2',	N'Ahmad Akra');

EXEC [api].[Relations__Save]
	@DefinitionId = @PartnerRLD,
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
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Relations] WHERE DefinitionId = @SupplierRLD;
EXEC [api].[Relations__Delete]
	@DefinitionId = @SupplierRLD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Suppliers: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @Relations; DELETE FROM @RelationUsers;
INSERT INTO @Relations([Index],	
	[Code],	[Name],					[TaxIdentificationNumber]) VALUES
(0,	N'S01',	N'Tellma',				NULL),
(1,	N'S02',	N'Ethio Telecom',		N'0000030603'),
(2,	N'S03',	N'Ethiopian Airlines',	NULL),
(3,	N'S04',	N'DARO',				NULL),
(4,	N'S05',	N'Wilfried Mofor',		NULL),
(5,	N'S06',	N'Yeshanew Gonfa',		N'0009683441'),
(6,	N'S07',	N'Abate GebretSadik Tekle',	N'0003833120');

EXEC [api].[Relations__Save]
	@DefinitionId = @SupplierRLD,
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
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Relations] WHERE DefinitionId = @EmployeeRLD;
EXEC [api].[Relations__Delete]
	@DefinitionId = @EmployeeRLD,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Employees: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @Relations; DELETE FROM @RelationUsers;
INSERT INTO @Relations([Index],	
	[Code],	[Name],				[CenterId], [TaxIdentificationNUmber]) VALUES
(0,	N'E001',N'Mohamad Akra',	@102C11,	N'0059603732'),
(1,	N'E002',N'Ahmad Akra',		@102C11,	NULL),
(2,	N'E003',N'Abdullah Ulber',	@102C11,	NULL),
(3,	N'E004',N'Yisak Fikadu',	@102C20,	N'0068469933'),
(4,	N'E005',N'Abrham Tenker',	@102C20,	N'0067651309');
EXEC [api].[Relations__Save]
	@DefinitionId = @EmployeeRLD,
	@Entities = @Relations,
	@RelationUsers = @RelationUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Employees: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;