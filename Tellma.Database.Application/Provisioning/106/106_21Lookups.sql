DELETE FROM @Lookups;

INSERT INTO @Lookups([Index],[Name],[Name2]) VALUES
(0, N'Cereals', N'ጥራጥሬዎች'),
(1, N'Pulses', N'በጥራጥሬ'),
(2, N'Oilseeds', N'ከቅባት'),
(3, N'Others', N'ሌሎች');

EXEC [api].Lookups__Save
@DefinitionId = @GrainClassificationLKD,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Grain Groups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],[Name],[Name2]) VALUES
(0, N'Maize', N'በቆሎ'),
(1, N'Wheat', N'ስንዴ'),
(2, N'Barley', N'ገብስ'),
(3, N'White pea beans', N'የነጭ የአተር ባቄላ'),
(4, N'Red Kidney beans', N'ቀይ የኩላሊት ባቄላ'),
(5, N'Red speckled beans', N'ቀይ ዥጉርጉር ባቄላ'),
(6, N'Soya beans', N'የአደንጓሬ ባቄላ'),
(7, N'chickpeas', N'ሽንብራ,'),
(8, N'Kabuli chickpeas', N'Kabuli ሽንብራ'),
(9, N'Fava beans', N'Fava ባቄላ'),
(10, N'Cream Beans', N'ክሬም የባቄላ'),
(11, N'Light Speckled beans', N'ፈካ ዥጉርጉር ባቄላ'),
(12, N'Chick beans', N'ጫጩት ባቄላ'),
(13, N'Black beans', N'ጥቁር ባቄላ'),
(14, N'Light brown', N'የፈካ ቡኒ'),
(15, N'Green Mung Beans', N'አረንጓዴ Mung የባቄላ'),
(16, N'lentils', N'ምስር'),
(17, N'Haricot Beans', N'ሄክታርና ባቄላ'),
(18, N'Niger seed', N'ኒጀር ዘር'),
(19, N'Sesame Seeds', N'የሰሊጥ ዘር'),
(20, N'Sunflower Seeds', N'የሱፍ አበባ ዘሮች'),
(21, N'Flaxseeds', N'ተልባ ዘሮች'),
(22, N'Peanut', N'ለዉዝ');

EXEC [api].Lookups__Save
@DefinitionId = @GrainTypeLKD,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Grain Types: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;						