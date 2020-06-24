INSERT INTO @Units([Index], [Name], [Name2], [Description],[Description2], [UnitType], [UnitAmount],[BaseAmount]) VALUES
(0, N'ea', N'እያንዳንዱ', N'Each', N'እያንዳንዱ', N'Count',1,1),
(2, N'pcs', N'ቁሶች', N'Pieces', N'ቁሶች', N'Count',1,1),
(3, N's', N'ሁለተኛ', N'second', N'ሁለተኛ', N'Time',3600,1),
(4, N'min', N'ደቂቃ', N'minute', N'ደቂቃ', N'Time',60,1),
(5, N'hr', N'ሰአት', N'Hour', N'ሰአት', N'Time',1,1),
(6, N'd', N'ቀን', N'Day', N'ቀን', N'Time',1,24),
(7, N'mo', N'ወር', N'Month', N'ወር', N'Time',1,1440),
(8, N'yr', N'አመት', N'Year', N'አመት', N'Time',1,8640),
(9, N'wd', N'የስራ ቀን', N'work day', N'የስራ ቀን', N'Time',1,8),
(10, N'wk', N'ሳምንት', N'week', N'ሳምንት', N'Time',1,168),
(11, N'wmo', N'የስራ ወር', N'work month', N'የስራ ወር', N'Time',1,208),
(12, N'wwk', N'የስራ ሳምንት', N'work week', N'የስራ ሳምንት', N'Time',1,48),
(13, N'wyr', N'የስራ ዓመት', N'work year', N'የስራ ዓመት', N'Time',1,2496),
(14, N'g', N'ፍርግርግ', N'Gram', N'ፍርግርግ', N'Mass',1000,1),
(15, N'kg', N'ኪሎግራም', N'Kilogram', N'ኪሎግራም', N'Mass',1,1),
(16, N'qn', N'ኩንታል', N'Quintal', N'ኩንታል', N'Mass',1,100),
(17, N'mt', N'ሜትሪክ ቶን', N'Metric ton', N'ሜትሪክ ቶን', N'Mass',1,1000),
(18, N'ltr', N'Liter', N'Liter', N'Liter', N'Volume',1,1),
(19, N'usg', N'የአሜሪካ ጋሎን', N'US Gallon', N'የአሜሪካ ጋሎን', N'Volume',1,3.785411784),
(20, N'cm', N'ሴንቲሜትር', N'Centimeter', N'ሴንቲሜትር', N'Distance',100,1),
(21, N'm', N'ሜትር', N'meter', N'ሜትር', N'Distance',1,1),
(22, N'km', N'ኪሎሜተር', N'Kilometer', N'ኪሎሜተር', N'Distance',1,1000);

EXEC [api].[Units__Save]
	@Entities = @Units,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Units: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

--Declarations
DECLARE @106ea INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'ea');
DECLARE @106pcs INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'pcs');
DECLARE @106s INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N's');
DECLARE @106min INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'min');
DECLARE @106hr INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'hr');
DECLARE @106d INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'd');
DECLARE @106mo INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'mo');
DECLARE @106yr INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'yr');
DECLARE @106wd INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'wd');
DECLARE @106wk INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'wk');
DECLARE @106wmo INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'wmo');
DECLARE @106wwk INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'wwk');
DECLARE @106wyr INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'wyr');
DECLARE @106g INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'g');
DECLARE @106kg INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'kg');
DECLARE @106qn INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'qn');
DECLARE @106mt INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'mt');
DECLARE @106ltr INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'ltr');
DECLARE @106usg INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'usg');
DECLARE @106cm INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'cm');
DECLARE @106m INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'm');
DECLARE @106km INT = (SELECT [Id] FROM dbo.Units WHERE [Name] = N'km');