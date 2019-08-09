DECLARE @MU [dbo].MeasurementUnitList;
INSERT INTO @MU (
[Name], [UnitType], [Description], [UnitAmount], [BaseAmount], [Code]) VALUES

(N'ea', N'Count', N'Each', 1, 1, NULL),
(N'share', N'Count', N'Shares', 1, 1, NULL),
(N'pcs', N'Count', N'Pieces', 1, 1, NULL),

(N's', N'Time', N'second', 1, 1, NULL),
(N'min', N'Time', N'minute', 1, 60, NULL),
(N'hr', N'Time', N'Hour', 1, 3600, NULL),
(N'd', N'Time', N'Day', 1, 86400, NULL),
(N'mo', N'Time', N'Month', 1, 2592000, NULL),
(N'yr', N'Time', N'Year', 1, 31104000, NULL),

(N'wd', N'Time', N'work day', 1, 8, NULL),
(N'wk', N'Time', N'week', 1, 604800, NULL),
(N'wmo', N'Time', N'work month', 1, 1248, NULL),
(N'wwk', N'Time', N'work week', 1, 48, NULL),
(N'wyr', N'Time', N'work year', 1, 14976, NULL),

(N'g', N'Mass', N'Gram', 1, 1, NULL),
(N'kg', N'Mass', N'Kilogram', 1, 1000, NULL),
(N'mt', N'Mass', N'Metric ton', 1, 1000000, NULL),

(N'ltr', N'Volume', N'Liter', 1, 1, NULL),
(N'usg', N'Volume', N'US Gallon', 1, 3.785411784, NULL),

(N'ETB', N'Currency', N'Ethiopian Birr', 27.8, 1, N'ETB'),
(N'USD', N'Currency', N'US Dollar', 1, 1, N'USD'),

(N'cm', N'Distance', N'Centimeter', 1, 1, NULL),
(N'm', N'Distance', N'meter', 1, 100, NULL),
(N'in', N'Distance', N'inch', 1, 2.541, NULL);

-- TODO: it is better to avoid defining any currency, except the functional, which we can retrieve from a table of currencies
-- and assume it is the base, so BaseAmount = UnitAmount = 1
IF NOT EXISTS(SELECT * FROM @MU WHERE [Name] = @FunctionalCurrency)
BEGIN
	INSERT INTO dbo.[MeasurementUnits]([UnitType], [Name], [Description])
	VALUES (N'Currency', @FunctionalCurrency, @FunctionalCurrency)
END

EXEC [api].[MeasurementUnits__Save]
	@Entities = @MU,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;