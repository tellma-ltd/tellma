DECLARE @MU [dbo].MeasurementUnitList;
INSERT INTO @MU ([Index],
	[Name], [UnitType], [Description], [UnitAmount], [BaseAmount], [Code]) VALUES

(0, N'ea', N'Count', N'Each', 1, 1, NULL),
(1, N'share', N'Count', N'Shares', 1, 1, NULL),
(2, N'pcs', N'Count', N'Pieces', 1, 1, NULL),

(3, N's', N'Time', N'second', 1, 1, NULL),
(4, N'min', N'Time', N'minute', 1, 60, NULL),
(5, N'hr', N'Time', N'Hour', 1, 3600, NULL),
(6, N'd', N'Time', N'Day', 1, 86400, NULL),
(7, N'mo', N'Time', N'Month', 1, 2592000, NULL),
(8, N'yr', N'Time', N'Year', 1, 31104000, NULL),

(9, N'wd', N'Time', N'work day', 1, 8, NULL),
(10, N'wk', N'Time', N'week', 1, 604800, NULL),
(11, N'wmo', N'Time', N'work month', 1, 1248, NULL),
(12, N'wwk', N'Time', N'work week', 1, 48, NULL),
(13, N'wyr', N'Time', N'work year', 1, 14976, NULL),

(14, N'g', N'Mass', N'Gram', 1, 1, NULL),
(15, N'kg', N'Mass', N'Kilogram', 1, 1000, NULL),
(16, N'mt', N'Mass', N'Metric ton', 1, 1000000, NULL),

(17, N'ltr', N'Volume', N'Liter', 1, 1, NULL),
(18, N'usg', N'Volume', N'US Gallon', 1, 3.785411784, NULL),

--(19, N'ETB', N'MonetaryValue', N'Ethiopian Birr', 27.8, 1, N'ETB'),
--(20, N'USD', N'MonetaryValue', N'US Dollar', 1, 1, N'USD'),

(21, N'cm', N'Distance', N'Centimeter', 100, 1, NULL),
(22, N'm', N'Distance', N'meter', 1, 1, NULL),
(23, N'km', N'Distance', N'Kilometer', 1, 1000, NULL),
(24, N'in', N'Distance', N'inch', 100, 2.541, NULL);

-- TODO: it is better to avoid defining any currency, except the functional, which we can retrieve from a table of currencies
-- and assume it is the base, so BaseAmount = UnitAmount = 1
IF NOT EXISTS(SELECT * FROM dbo.Currencies WHERE [Id] = @FunctionalCurrency)
	INSERT INTO dbo.Currencies([Id], [Name], [Description]) VALUES (@FunctionalCurrency,@FunctionalCurrency, @FunctionalCurrency);
IF NOT EXISTS(SELECT * FROM dbo.Currencies WHERE [Id] = N'ETB')
	INSERT INTO dbo.Currencies([Id], [Name], [Description]) VALUES (N'ETB', N'ETB', N'Ethiopian Birr');
IF NOT EXISTS(SELECT * FROM dbo.Currencies WHERE [Id] = N'USD')
	INSERT INTO dbo.Currencies([Id], [Name], [Description]) VALUES (N'USD', N'USD', N'US Dollar');

EXEC [api].[MeasurementUnits__Save]
	@Entities = @MU,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;