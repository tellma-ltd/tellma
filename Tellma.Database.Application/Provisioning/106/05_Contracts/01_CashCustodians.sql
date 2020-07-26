--DELETE FROM @CashOnHandContracts;
--DELETE FROM @RelationUsers;
--INSERT INTO @CashOnHandContracts([Index], [Name], [Name2], [CenterId]) VALUES
--(0,			N'Cash 1',	NULL, @106C_HeadOffice);
--INSERT INTO @RelationUsers([Index], [HeaderIndex], [UserId]) VALUES
--(0,0,@AdminUserId)

--EXEC [api].[Relations__Save]
--	@DefinitionId = @EmployeeCD,
--	@Entities = @CashOnHandContracts,
--	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--IF @ValidationErrorsJson IS NOT NULL 
--BEGIN
--	Print 'Cash on hand contracts: Inserting: ' + @ValidationErrorsJson
--	GOTO Err_Label;
--END;

--DELETE FROM @CashOnHandContracts;
--DELETE FROM @RelationUsers;
--INSERT INTO @CashOnHandContracts([Index], [Name], [Name2], [CenterId], [CurrencyId]) VALUES
--(0,			N'Bank 1',	NULL, @106C_HeadOffice, @ETB),
--(1,			N'Bank 2',	NULL, @106C_HeadOffice, @USD);
--INSERT INTO @RelationUsers([Index], [HeaderIndex], [UserId]) VALUES
--(0,0,@AdminUserId),
--(0,1,@AdminUserId)

--EXEC [api].[Relations__Save]
--	@DefinitionId = @BankCD,
--	@Entities = @CashOnHandContracts,
--	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--IF @ValidationErrorsJson IS NOT NULL 
--BEGIN
--	Print 'Cash on hand contracts: Inserting: ' + @ValidationErrorsJson
--	GOTO Err_Label;
--END;