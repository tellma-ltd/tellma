INSERT INTO @Units([Index], [Code], [Name], [Description], [UnitType], [UnitAmount],[BaseAmount]) VALUES
(0, N'pure', N'pure', N'Unique Instance', N'Pure',1,1),
(1, N'ea', N'ea', N'Each', N'Count',1,1),
(2, N'pcs', N'pcs', N'Pieces', N'Count',1,1),
(3, N'pkt500', N'pkt/500', N'Packet (of 500 count)', N'Count',500,1),
(4, N's', N's', N'second', N'Time',3600,1),
(5, N'min', N'min', N'minute', N'Time',60,1),
(6, N'hr', N'hr', N'Hour', N'Time',1,1),
(7, N'd', N'd', N'Day', N'Time',1,24),
(8, N'mo', N'mo', N'Month', N'Time',1,1440),
(9, N'yr', N'yr', N'Year', N'Time',1,8640),
(10, N'wd', N'wd', N'work day', N'Time',1,8),
(11, N'wk', N'wk', N'week', N'Time',1,168),
(12, N'wmo', N'wmo', N'work month', N'Time',1,208),
(13, N'wwk', N'wwk', N'work week', N'Time',1,48),
(14, N'wyr', N'wyr', N'work year', N'Time',1,2496),
(15, N'g', N'g', N'Gram', N'Mass',1000,1),
(16, N'kg', N'kg', N'Kilogram', N'Mass',1,1),
(17, N'qn50', N'qn/50', N'Quintal (50 Kg)', N'Mass',1,50),
(18, N'qn100', N'qn/100', N'Quintal (100 Kg)', N'Mass',1,100),
(19, N'mt', N'mt', N'Metric ton', N'Mass',1,1000),
(20, N'ltr', N'ltr', N'Liter', N'Volume',1,1),
(21, N'usg', N'usg', N'US Gallon', N'Volume',1,3.785411784),
(22, N'cm', N'cm', N'Centimeter', N'Distance',100,1),
(23, N'm', N'm', N'meter', N'Distance',1,1),
(24, N'km', N'km', N'Kilometer', N'Distance',1,1000);

	EXEC [api].[Units__Save]
		@Entities = @Units,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Units: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

--Declarations
DECLARE @pure INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'pure');
DECLARE @ea INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'ea');
DECLARE @pcs INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'pcs');
DECLARE @pkt500 INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'pkt500');
DECLARE @s INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N's');
DECLARE @min INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'min');
DECLARE @hr INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'hr');
DECLARE @d INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'd');
DECLARE @mo INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'mo');
DECLARE @yr INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'yr');
DECLARE @wd INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'wd');
DECLARE @wk INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'wk');
DECLARE @wmo INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'wmo');
DECLARE @wwk INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'wwk');
DECLARE @wyr INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'wyr');
DECLARE @g INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'g');
DECLARE @kg INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'kg');
DECLARE @qn50 INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'qn50');
DECLARE @qn100 INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'qn100');
DECLARE @mt INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'mt');
DECLARE @ltr INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'ltr');
DECLARE @usg INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'usg');
DECLARE @cm INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'cm');
DECLARE @m INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'm');
DECLARE @km INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'km');