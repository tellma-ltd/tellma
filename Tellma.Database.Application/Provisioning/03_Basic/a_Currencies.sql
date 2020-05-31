DECLARE @Currencies CurrencyList;

IF @DB = N'100' -- ACME, USD, en/ar/zh
	PRINT N'Tellma'
--INSERT INTO @Currencies([Index],
--	[Id],	[Name],			[Name2],			[Description],			[Description2],		[E]) VALUES
--(0, N'USD', N'USD',			N'دولار',			N'United States Dollar',N'دولار أمريكي',		2),
--(1, N'ETB', N'Birr',		N'بر',				N'Ethiopian Birr',		N'بر أثيوبي',		2),
--(2, N'GBP', N'B.Pound',		N'جنيه ب',			N'Sterling Pound',		N'جنيه استرليني',	2),
--(3, N'AED', N'Dirham',		N'درهم',			N'Emirates Dirham',		N'درهم إماراتي',	2),
--(4, N'SAR', N'Riyal',		N'ريال',				N'Saudi Riyal',			N'ريال سعودي',			2),
--(5, N'CNY', N'Yuan',		N'يوان',			N'Chinese Yuan',		N'يوان صيني',		2),
--(6, N'SDG', N'S.Pound',		N'جنيه س',			N'Sudanese Pound',		N'جنيه سوداني',		2),
--(7, N'EUR', N'EUR',			N'يورو',			N'Euro',				N'يورو',			2);

ELSE IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @Currencies([Index], [Id], [Name], [Name2], [Name3], [Description], [Description2], [Description3], E) VALUES
	(784, N'AED', N'UAE Dirham', N'درهم إماراتي', N'UAE Dirham', N'United Arab Emirates Dirham', N'درهم إماراتي', N'United Arab Emirates Dirham', 2),
	(682, N'SAR', N'KSA Riyal', N'ريال سعودي', N'KSA Riyal', N'Saudi Riyal', N'ريال سعودي', N'Saudi Riyal', 2),
	(938, N'SDG', N'SD Pound', N'جنيه سوداني', N'SD Pound', N'Sudanese Pound', N'جنيه سوداني', N'Sudanese Pound', 2);

ELSE IF @DB = N'102' -- Banan ET, ETB, en
	INSERT INTO @Currencies([Index], [Id],	[Name],			[Description],				[E]) VALUES
	(1, N'USD', N'USD',			N'United States Dollar',	2);

ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	INSERT INTO @Currencies([Index], [Id], [Name], [Name2], [Name3], [Description], [Description2], [Description3], E) VALUES
	(156, N'CNY', N'yuan', N'يوان', N'yuan', N'Renminbi (Chinese) yuan', N'الرنمينبي (صيني) يوان', N'Renminbi (Chinese) yuan', 2),
	(840, N'USD', N'US Dollar', N'دولار أمريكي', N'US Dollar', N'United States dollar', N'دولار الولايات المتحدة', N'United States dollar', 2);

ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @Currencies([Index], [Id], [Name], [Name2], [Name3], [Description], [Description2], [Description3], E) VALUES
	(978, N'EUR', N'Euro', N'ዩሮ', N'Euro', N'Euro', N'ዩሮ', N'Euro', 2),
	(840, N'USD', N'US Dollar', N'የአሜሪካ ዶላር', N'US Dollar', N'United States dollar', N'የአሜሪካ ዶላር', N'United States dollar', 2);

ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
INSERT INTO @Currencies([Index], [Id], [Name], [Name2], [Name3], [Description], [Description2], [Description3], E) VALUES
	(784, N'AED', N'UAE Dirham', N'درهم إماراتي', N'UAE Dirham', N'United Arab Emirates Dirham', N'درهم إماراتي', N'United Arab Emirates Dirham', 2),
	(978, N'EUR', N'Euro', N'يورو', N'Euro', N'Euro', N'اليورو', N'Euro', 2),
	(840, N'USD', N'US Dollar', N'دولار أمريكي', N'US Dollar', N'United States dollar', N'دولار الولايات المتحدة', N'United States dollar', 2);

ELSE IF @DB = N'106' -- Soreti, ETB, en/am
	INSERT INTO @Currencies([Index], [Id], [Name], [Name2], [Name3], [Description], [Description2], [Description3], E) VALUES
	(978, N'EUR', N'Euro', N'ዩሮ', N'Euro', N'Euro', N'ዩሮ', N'Euro', 2),
	(840, N'USD', N'US Dollar', N'የአሜሪካ ዶላር', N'US Dollar', N'United States dollar', N'የአሜሪካ ዶላር', N'United States dollar', 2);

EXEC [api].Currencies__Save
	@Entities = @Currencies,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Currencies: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;						
EXEC sys.sp_set_session_context 'FunctionalCurrencyId', @FunctionalCurrencyId;

IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO dbo.ExchangeRates(
	[CurrencyId],	[ValidAsOf],	[AmountInCurrency], [AmountInFunctional]) VALUES
	(N'SDG',		N'2019.01.01',	100,				1),
	(N'SAR',		N'2019.01.01',	3.75,				1);