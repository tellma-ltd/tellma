-- Banks
SET @DefinitionId = @BankLKD; DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],[Code],[Name]) VALUES
(1, N'ABAYETAA', N'Abay Bank S.C.'),
(2, N'ABSCETAA', N'Addis International Bank'),
(3, N'AWINETAA', N'Awash International Bank'),
(4, N'ABYSETAA', N'Bank of Abyssinia'),
(5, N'BERHETAA', N'Berhan International Bank'),
(6, N'BUNAETAA', N'Bunna International Bank'),
(7, N'CBETETAA', N'Commercial Bank of Ethiopia'),
(8, N'CBORETAA', N'Cooperative Bank of Oromia(s.c.)'),
(9, N'DASHETAA', N'Dashen Bank'),
(10, N'DEGAETAA', N'Debub Global Bank'),
(11, N'ENATETAA', N'Enat Bank'),
(12, N'LIBSETAA', N'Lion International Bank'),
(13, N'NIBIETTA', N'Nib International Bank'),
(14, N'ORIRETAA', N'Oromia International Bank'),
(15, N'UNTDETAA', N'United Bank'),
(16, N'WEGAETAA', N'Wegagen Bank'),
(17, N'ZEMEETAA', N'Zemen Bank'),
(18, N'DBEETAA', N'Development Bank of Ethiopia');

EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Banks: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- Bank Account Type
SET @DefinitionId = @BankAccountTypeLKD; DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],[Code],[Name], [Name2]) VALUES
(0,N'CR', N'Current', N'የአሁኑ');
EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Bank Account Types: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

