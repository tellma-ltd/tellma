DECLARE @Currencies CurrencyList;
INSERT INTO @Currencies([Index],
	[Id],	[Name],			[Name2],			[E]) VALUES
(0, N'USD', N'US Dollar',	N'دولار أمريكي',		2),
(1, N'ETB', N'ET Birr',		N'بر أثيوبي',		2),
(2, N'GBP', N'Sterling Pound',N'جنيه استرليني',2),
(3, N'AED', N'UAE Dirham',	N'درهم إماراتي',	2),
(4, N'JPY', N'JP Yen',		N'ين ياباني',		2);

EXEC [api].Currencies__Save
	@Entities = @Currencies,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Currencies: Inserting'
	GOTO Err_Label;
END;						

DECLARE @ActiveCurrencies StringList;
INSERT INTO @ActiveCurrencies VALUES 
(@FunctionalCurrencyId),
(N'USD');

EXEC dal.Currencies__Activate
	@Ids = @ActiveCurrencies,
	@IsActive = 1;

EXEC master.sys.sp_set_session_context 'FunctionalCurrencyId', @FunctionalCurrencyId;

DECLARE @DeletedCurrencies IndexedStringList
INSERT INTO @DeletedCurrencies
([Index],	[Id]) VALUES 
(0,			N'GBP'), 
(1,			N'JPY');

EXEC [api].Currencies__Delete
	@IndexedIds = @DeletedCurrencies,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @DebugCurrencies = 1
BEGIN
	SELECT * FROM map.Currencies();
	SELECT * FROM map.Resources();
END