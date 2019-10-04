DECLARE @Currencies CurrencyList;
INSERT INTO @Currencies([Index], [Id], [Name], [Name2], [E]) VALUES
(0, N'USD', N'US Dollar',N'دولار أمريكي', 2),
(1, N'ETB', N'ET Birr', N'بر أثيوبي', 2),
(2, N'GBP', N'Sterling Pound',N'جنيه استرليني', 2),
(3, N'AED', N'UAE Dirham', N'درهم إماراتي', 2);

EXEC dal.Currencies__Save
	@Entities = @Currencies;

DECLARE @ActiveCurrencies StringList;
INSERT INTO @ActiveCurrencies VALUES 
(@FunctionalCurrency),
(N'USD');

EXEC dal.Currencies__Activate
	@Ids = @ActiveCurrencies,
	@IsActive = 1;