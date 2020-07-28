DELETE FROM @SafeCustodies;
DELETE FROM @RelationUsers;
INSERT INTO @SafeCustodies([Index], [Name], [Name2], [CenterId]) VALUES
(0,			N'Cash 1',	NULL, @106C_HeadOffice);
INSERT INTO @RelationUsers([Index], [HeaderIndex], [UserId]) VALUES
(0,0,@AdminUserId)

EXEC [api].[Relations__Save]
	@DefinitionId = @SafeCD,
	@Entities = @SafeCustodies,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Safes Custodies: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DELETE FROM @BankAccountCustodies;
DELETE FROM @RelationUsers;
INSERT INTO @BankAccountCustodies([Index], [Name], [Name2], [CenterId], [CurrencyId]) VALUES
(0,			N'Bank 1',	NULL, @106C_HeadOffice, @ETB),
(1,			N'Bank 2',	NULL, @106C_HeadOffice, @USD);
INSERT INTO @RelationUsers([Index], [HeaderIndex], [UserId]) VALUES
(0,0,@AdminUserId),
(0,1,@AdminUserId)

EXEC [api].[Relations__Save]
	@DefinitionId = @BankAccountCD,
	@Entities = @BankAccountCustodies,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Bank Accounts Custodies: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;