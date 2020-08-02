-- Banks
SET @DefinitionId = @BankLKD; DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],[Code],[Name], [Name2]) VALUES
(1, N'ABAYETAA', N'Abay Bank S.C.', NULL),
(2, N'ABSCETAA', N'Addis International Bank', NULL),
(3, N'AWINETAA', N'Awash International Bank', N'አዋሽ ባንክ'),
(4, N'ABYSETAA', N'Bank of Abyssinia', N'የአቢሲኒያ ባንክ'),
(5, N'BERHETAA', N'Berhan International Bank', NULL),
(6, N'BUNAETAA', N'Bunna International Bank', NULL),
(7, N'CBETETAA', N'Commercial Bank of Ethiopia', N'የኢትዮጵያ ንግድ ባንክ'),
(8, N'CBORETAA', N'Cooperative Bank of Oromia(s.c.)', N'የኦሮሚያ ህብረት ስራ ባንክ'),
(9, N'DASHETAA', N'Dashen Bank', N'ዳሽን ባንክ'),
(10, N'DEGAETAA', N'Debub Global Bank', NULL),
(11, N'ENATETAA', N'Enat Bank', NULL),
(12, N'LIBSETAA', N'Lion International Bank', NULL),
(13, N'NIBIETTA', N'Nib International Bank', N'ንብ ኢንተርናሽናል ባንክ'),
(14, N'ORIRETAA', N'Oromia International Bank', N'ኦሮሚያ ኢንተርናሽናል ባንክ'),
(15, N'UNTDETAA', N'United Bank', N'የህብረት ባንክ'),
(16, N'WEGAETAA', N'Wegagen Bank', N'ወጋገን ባንክ'),
(17, N'ZEMEETAA', N'Zemen Bank', N'የዘመን ባንክ'),
(18, N'DBEETAA', N'Development Bank of Ethiopia', N'የኢትዮጵያ ልማት ባንክ');

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
(0,N'RB', N'Retention B', N'ማቆየት ለ'),
(1,N'RA', N'Retention A', N'ማቆየት ሀ'),
(2,N'EPI', N'Ecx Pay In', N'ECX ክፍያ'),
(3,N'IF', N'Interest Free', N'ከወለድ ነፃ'),
(4,N'R90', N'Retention 90%', N'90% ማቆየት'),
(5,N'CR', N'Current', N'የአሁኑ'),
(6,N'EOD', N'Export Overdraft', N'በረቂቅ ላይ ይላኩ'),
(7,N'FN', N'Financing', N'ፋይናንስ'),
(8,N'IOD', N'Import Overdraft', N'በረቂቅ ላይ ያስገቡ'),
(9,N'MR', N'Muraba', N'ሙራባሀ'),
(10,N'OD', N'Overdraft', N'ከመጠን በላይ ማለፍ'),
(11,N'RV', N'Revolving', N'መሽከርከር'),
(12,N'SOD', N'Special Overdraft', N'ልዩ ከመጠን በላይ ማለፍ');

EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Bank Account Types: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;


-- Grain Groups
SET @DefinitionId = @GrainClassificationLKD; DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],[Code], [Name],[Name2]) VALUES
(0, N'C', N'Cereals', N'ጥራጥሬዎች'),
(1, N'P', N'Pulses', N'በጥራጥሬ'),
(2, N'S', N'Oilseeds', N'ከቅባት'),
(3, N'X', N'Others', N'ሌሎች');

EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Grain Groups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],[Code],[Name],[Name2]) VALUES
(0,N'M', N'Maize', N'በቆሎ'),
(1,N'W', N'Wheat', N'ስንዴ'),
(2,N'B', N'Barley', N'ገብስ'),
(3,N'WPB', N'White pea beans', N'የነጭ የአተር ባቄላ'),
(4,N'RKB', N'Red Kidney beans', N'ቀይ የኩላሊት ባቄላ'),
(5,N'RSB', N'Red speckled beans', N'ቀይ ዥጉርጉር ባቄላ'),
(6,N'SB', N'Soya beans', N'የአደንጓሬ ባቄላ'),
(7,N'CP', N'chickpeas', N'ሽንብራ,'),
(8,N'KCP', N'Kabuli chickpeas', N'Kabuli ሽንብራ'),
(9,N'FB', N'Fava beans', N'Fava ባቄላ'),
(10,N'CRB', N'Cream Beans', N'ክሬም የባቄላ'),
(11,N'LSB', N'Light Speckled beans', N'ፈካ ዥጉርጉር ባቄላ'),
(13,N'BB', N'Black beans', N'ጥቁር ባቄላ'),
(14,N'LB', N'Light brown', N'የፈካ ቡኒ'),
(15,N'GMB', N'Green Mung Beans', N'አረንጓዴ Mung የባቄላ'),
(16,N'L', N'lentils', N'ምስር'),
(17,N'HB', N'Haricot Beans', N'ሄክታርና ባቄላ'),
(18,N'NS', N'Niger seed', N'ኒጀር ዘር'),
(19,N'SS', N'Sesame Seeds', N'የሰሊጥ ዘር'),
(20,N'SunS', N'Sunflower Seeds', N'የሱፍ አበባ ዘሮች'),
(21,N'FS', N'Flaxseeds', N'ተልባ ዘሮች'),
(22,N'P', N'Peanut', N'ለዉዝ'),
(23,N'LS', N'Linseeds', N'ሊን ዘሮች');

EXEC [api].Lookups__Save
@DefinitionId = @GrainTypeLKD,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Grain Types: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;