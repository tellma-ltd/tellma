-- Bank Branch
DELETE FROM @IndexedIds; SET @DefinitionId = @BankBranchRLD;
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Relations] WHERE DefinitionId = @DefinitionId;
EXEC [api].[Relations__Delete]
	@DefinitionId = @DefinitionId,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Bank Branches: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @Relations; DELETE FROM @RelationUsers;
INSERT INTO @Relations([Index],	
	[Code], [Name]) VALUES
(0,	N'BB01',N'CBE - Bole'),
(1,	N'BB02',N'CBE - Girgi');
EXEC [api].[Relations__Save]
	@DefinitionId = @DefinitionId,
	@Entities = @Relations,
	@RelationUsers = @RelationUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Bank Branches: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- Supplier
DELETE FROM @IndexedIds; SET @DefinitionId = @SupplierRLD;
INSERT INTO @IndexedIds SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.[Relations] WHERE DefinitionId = @DefinitionId;
EXEC [api].[Relations__Delete]
	@DefinitionId = @DefinitionId,
	@IndexedIds = @IndexedIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Default Suppliers: Deleting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
DELETE FROM @Relations; DELETE FROM @RelationUsers;
INSERT INTO @Relations([Index],	
	[Code], [Name]) VALUES
(0,	N'S1',N'Supplier 1'),
(1,	N'S2',N'Supplier 2');
EXEC [api].[Relations__Save]
	@DefinitionId = @DefinitionId,
	@Entities = @Relations,
	@RelationUsers = @RelationUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Suppliers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
