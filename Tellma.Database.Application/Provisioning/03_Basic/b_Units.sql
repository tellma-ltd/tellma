DECLARE @Units [dbo].UnitList;


IF @DB = N'100' -- ACME, USD, en/ar/zh
INSERT INTO @Units ([Index],
	[Name], [Name2],	[Name3],	[UnitType], [Description],	[UnitAmount],	[BaseAmount]) VALUES
(0, N'ea',	NULL,		NULL,		N'Count',	N'Each',		1,				1),
(1, N'share', NULL,		NULL,		N'Count',	N'Shares',		1,				1),
(2, N'pcs', N'قطعة',	NULL,		N'Count',	N'Pieces',		1,				1),

(3, N's',	N'ث',		NULL,		N'Time',	N'second',		1,				1),
(4, N'min',	N'د',		NULL,		N'Time',	N'minute',		1,				60),
(5, N'hr',	N'س',		NULL,		N'Time',	N'Hour',		1,				3600),
(6, N'd',	N'يوم',		NULL,		N'Time',	N'Day',			1,				86400),
(7, N'mo',	N'شهر',		NULL,		N'Time',	N'Month',		1,				2592000),
(8, N'yr',	N'سنة',		NULL,		N'Time',	N'Year',		1,				31104000),

(9, N'wd',	NULL,		NULL,		N'Time',	N'work day',	1,				8),
(10, N'wk', NULL,		NULL,		N'Time',	N'week',		1,				604800),
(11, N'wmo',NULL,		NULL,		N'Time',	N'work month',	1,				1248),
(12, N'wwk',NULL,		NULL,		N'Time',	N'work week',	1,				48),
(13, N'wyr', NULL,		NULL,		N'Time',	N'work year',	1,				14976),

(14, N'g', N'غ',		NULL,		N'Mass',	N'Gram',		1,				1),
(15, N'kg', N'كغ',		NULL,		N'Mass',	N'Kilogram',	1,				1000),
(16, N'mt', N'طن',		NULL,		N'Mass',	N'Metric ton',	1,				1000000),
 
(17, N'ltr', N'لتر',	NULL,		N'Volume',	N'Liter',		1,				1),
(18, N'usg', NULL,		NULL,		N'Volume',	N'US Gallon',	1,				3.785411784),

(21, N'cm', N'سم',		NULL,		N'Distance',N'Centimeter',	100,			1),
(22, N'm', N'م',		NULL,		N'Distance',N'meter',		1,				1),
(23, N'km', N'كم',		NULL,		N'Distance',N'Kilometer',	1,				1000),
(24, N'in', NULL,		NULL,		N'Distance',N'inch',		100,			2.541);

ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
INSERT INTO @Units ([Index],
	[Name], [Name2],	[Name3],	[UnitType], [Description],	[UnitAmount],	[BaseAmount]) VALUES
(0, N'ea',	NULL,		NULL,		N'Count',	N'Each',		1,				1),
(1, N'share', NULL,		NULL,		N'Count',	N'Shares',		1,				1),
(2, N'pcs', N'قطعة',	NULL,		N'Count',	N'Pieces',		1,				1),

(3, N's',	N'ث',		NULL,		N'Time',	N'second',		1,				1),
(4, N'min',	N'د',		NULL,		N'Time',	N'minute',		1,				60),
(5, N'hr',	N'س',		NULL,		N'Time',	N'Hour',		1,				3600),
(6, N'd',	N'يوم',		NULL,		N'Time',	N'Day',			1,				86400),
(7, N'mo',	N'شهر',		NULL,		N'Time',	N'Month',		1,				2592000),
(8, N'yr',	N'سنة',		NULL,		N'Time',	N'Year',		1,				31104000),

(9, N'wd',	NULL,		NULL,		N'Time',	N'work day',	1,				8),
(10, N'wk', NULL,		NULL,		N'Time',	N'week',		1,				604800),
(11, N'wmo',NULL,		NULL,		N'Time',	N'work month',	1,				1248),
(12, N'wwk',NULL,		NULL,		N'Time',	N'work week',	1,				48),
(13, N'wyr', NULL,		NULL,		N'Time',	N'work year',	1,				14976);
UPDATE @Units SET [Code] = 
	CASE
		WHEN [Index] = 6 THEN N'DAY'
		WHEN [Index] = 7 THEN N'MONTH'
		WHEN [Index] = 8 THEN N'YEAR'
	END
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
INSERT INTO @Units ([Index],
	[Name], [Name2],	[Name3],	[UnitType], [Description],	[UnitAmount],	[BaseAmount]) VALUES
(0, N'ea',	NULL,		NULL,		N'Count',	N'Each',		1,				1),
(1, N'share', NULL,		NULL,		N'Count',	N'Shares',		1,				1),
(2, N'pcs', N'قطعة',	NULL,		N'Count',	N'Pieces',		1,				1),

(3, N's',	N'ث',		NULL,		N'Time',	N'second',		1,				1),
(4, N'min',	N'د',		NULL,		N'Time',	N'minute',		1,				60),
(5, N'hr',	N'س',		NULL,		N'Time',	N'Hour',		1,				3600),
(6, N'd',	N'يوم',		NULL,		N'Time',	N'Day',			1,				86400),
(7, N'mo',	N'شهر',		NULL,		N'Time',	N'Month',		1,				2592000),
(8, N'yr',	N'سنة',		NULL,		N'Time',	N'Year',		1,				31104000),

(9, N'wd',	NULL,		NULL,		N'Time',	N'work day',	1,				8),
(10, N'wk', NULL,		NULL,		N'Time',	N'week',		1,				604800),
(11, N'wmo',NULL,		NULL,		N'Time',	N'work month',	1,				1248),
(12, N'wwk',NULL,		NULL,		N'Time',	N'work week',	1,				48),
(13, N'wyr', NULL,		NULL,		N'Time',	N'work year',	1,				14976);

ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
INSERT INTO @Units ([Index],
	[Name], [Name2],	[Name3],	[UnitType], [Description],	[UnitAmount],	[BaseAmount]) VALUES
(0, N'ea',	NULL,		NULL,		N'Count',	N'Each',		1,				1),
(1, N'share', NULL,		NULL,		N'Count',	N'Shares',		1,				1),
(2, N'pcs', N'قطعة',	NULL,		N'Count',	N'Pieces',		1,				1),

(3, N's',	N'ث',		NULL,		N'Time',	N'second',		1,				1),
(4, N'min',	N'د',		NULL,		N'Time',	N'minute',		1,				60),
(5, N'hr',	N'س',		NULL,		N'Time',	N'Hour',		1,				3600),
(6, N'd',	N'يوم',		NULL,		N'Time',	N'Day',			1,				86400),
(7, N'mo',	N'شهر',		NULL,		N'Time',	N'Month',		1,				2592000),
(8, N'yr',	N'سنة',		NULL,		N'Time',	N'Year',		1,				31104000),

(9, N'wd',	NULL,		NULL,		N'Time',	N'work day',	1,				8),
(10, N'wk', NULL,		NULL,		N'Time',	N'week',		1,				604800),
(11, N'wmo',NULL,		NULL,		N'Time',	N'work month',	1,				1248),
(12, N'wwk',NULL,		NULL,		N'Time',	N'work week',	1,				48),
(13, N'wyr', NULL,		NULL,		N'Time',	N'work year',	1,				14976),

(14, N'g', N'غ',		NULL,		N'Mass',	N'Gram',		1,				1),
(15, N'kg', N'كغ',		NULL,		N'Mass',	N'Kilogram',	1,				1000),
(16, N'mt', N'طن',		NULL,		N'Mass',	N'Metric ton',	1,				1000000),
 
(17, N'ltr', N'لتر',	NULL,		N'Volume',	N'Liter',		1,				1),
(18, N'usg', NULL,		NULL,		N'Volume',	N'US Gallon',	1,				3.785411784),

(21, N'cm', N'سم',		NULL,		N'Distance',N'Centimeter',	100,			1),
(22, N'm', N'م',		NULL,		N'Distance',N'meter',		1,				1),
(23, N'km', N'كم',		NULL,		N'Distance',N'Kilometer',	1,				1000),
(24, N'in', NULL,		NULL,		N'Distance',N'inch',		100,			2.541);

ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
INSERT INTO @Units ([Index],
	[Name], [Name2],	[Name3],	[UnitType], [Description],	[UnitAmount],	[BaseAmount]) VALUES
(0, N'ea',	NULL,		NULL,		N'Count',	N'Each',		1,				1),
(1, N'share', NULL,		NULL,		N'Count',	N'Shares',		1,				1),
(2, N'pcs', N'قطعة',	NULL,		N'Count',	N'Pieces',		1,				1),

(3, N's',	N'ث',		NULL,		N'Time',	N'second',		1,				1),
(4, N'min',	N'د',		NULL,		N'Time',	N'minute',		1,				60),
(5, N'hr',	N'س',		NULL,		N'Time',	N'Hour',		1,				3600),
(6, N'd',	N'يوم',		NULL,		N'Time',	N'Day',			1,				86400),
(7, N'mo',	N'شهر',		NULL,		N'Time',	N'Month',		1,				2592000),
(8, N'yr',	N'سنة',		NULL,		N'Time',	N'Year',		1,				31104000),

(9, N'wd',	NULL,		NULL,		N'Time',	N'work day',	1,				8),
(10, N'wk', NULL,		NULL,		N'Time',	N'week',		1,				604800),
(11, N'wmo',NULL,		NULL,		N'Time',	N'work month',	1,				1248),
(12, N'wwk',NULL,		NULL,		N'Time',	N'work week',	1,				48),
(13, N'wyr', NULL,		NULL,		N'Time',	N'work year',	1,				14976),

(14, N'g', N'غ',		NULL,		N'Mass',	N'Gram',		1,				1),
(15, N'kg', N'كغ',		NULL,		N'Mass',	N'Kilogram',	1,				1000),
(16, N'mt', N'طن',		NULL,		N'Mass',	N'Metric ton',	1,				1000000),
 
(17, N'ltr', N'لتر',	NULL,		N'Volume',	N'Liter',		1,				1),
(18, N'usg', NULL,		NULL,		N'Volume',	N'US Gallon',	1,				3.785411784),

(21, N'cm', N'سم',		NULL,		N'Distance',N'Centimeter',	100,			1),
(22, N'm', N'م',		NULL,		N'Distance',N'meter',		1,				1),
(23, N'km', N'كم',		NULL,		N'Distance',N'Kilometer',	1,				1000),
(24, N'in', NULL,		NULL,		N'Distance',N'inch',		100,			2.541);

ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
INSERT INTO @Units ([Index],
	[Name],		[Name2],	[UnitType], [Description],	[UnitAmount],	[BaseAmount]) VALUES
(0, N'ea',		N'وحدة',	N'Count',	N'Each',		1,				1),
--(1, N'pcs',		N'قطعة',	N'Count',	N'Pieces',	1,				1),
(2, N'500pkt',	N'رزمة500',	N'Count',	N'Packet (500)',1,				500),

--(3, N's',	N'ث',		N'Time',	N'second',		1,				1),
--(4, N'min',	N'د',		N'Time',	N'minute',		1,				60),
(5, N'hr',	N'س',		N'Time',	N'Hour',		1,				3600),
(6, N'd',	N'يوم',		N'Time',	N'Day',			1,				86400),
(7, N'mo',	N'شهر',		N'Time',	N'Month',		1,				2592000),
(8, N'yr',	N'سنة',		N'Time',	N'Year',		1,				31104000),

(9, N'wd',	N'بوم عمل',	N'Time',	N'work day',	1,				8),
(10, N'wk', N'أسبوع',	N'Time',	N'week',		1,				604800),
(11, N'wmo',N'شهر عمل',	N'Time',	N'work month',	1,				1248),
(12, N'wwk',N'أسبوع عمل',N'Time',	N'work week',	1,				48),
(13, N'wyr', N'سنة عمل',N'Time',	N'work year',	1,				14976),

(14, N'g', N'غ',		N'Mass',	N'Gram',		1,				1),
(15, N'kg', N'كغ',		N'Mass',	N'Kilogram',	1,				1000),
(16, N'mt', N'طن',		N'Mass',	N'Metric ton',	1,				1000000);
 
--(17, N'ltr', N'لتر',	N'Volume',	N'Liter',		1,				1),
--(18, N'usg', 			N'Volume',	N'US Gallon',	1,				3.785411784),

--(21, N'cm', N'سم',		N'Distance',N'Centimeter',	100,			1),
--(22, N'm',	N'م',		N'Distance',N'meter',		1,				1),
--(23, N'km', N'كم',		N'Distance',N'Kilometer',	1,				1000),
--(24, N'in', N'إنش',		N'Distance',N'inch',		100,			2.541);

EXEC [api].[Units__Save]
	@Entities = @Units,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Units: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DECLARE @WorkMonth INT, @Hour INT, @Month INT, @Year INT;
SELECT @WorkMonth = [Id] FROM dbo.[Units] WHERE [Name] = N'wmo';
SELECT @Month = [Id] FROM dbo.[Units] WHERE [Name] = N'mo';
SELECT @Hour = [Id] FROM dbo.[Units] WHERE [Name] = N'hr';
SELECT @Year = [Id] FROM dbo.[Units] WHERE [Name] = N'yr';