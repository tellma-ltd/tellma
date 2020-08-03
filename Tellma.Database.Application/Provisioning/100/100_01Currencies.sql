-- Activate Currencies in Use
INSERT INTO @IndexedCurrencyIds
([Index],	[Id]) VALUES
(0,			@SDG),
(1,			@USD),
(2,			@SAR),
(3,			@AED);
EXEC [api].[Currencies__Activate]
	@IndexedIds = @IndexedCurrencyIds,
	@IsActive = 1,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Currencies Activating: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- Make active currency names shorter
DELETE FROM @Currencies
INSERT INTO @Currencies([Index],			[NumericCode], [Id], [Name], [Description],[E]) 
 
SELECT  ROW_NUMBER() OVER(ORDER BY [Id]),	[NumericCode], [Id], [Name], [Description],[E]
FROM dbo.[Currencies]
WHERE [Id] IN (SELECT[Id] FROM @IndexedCurrencyIds);

UPDATE @Currencies SET [Name] = N'Pound', [Name2] = N'جنيه' WHERE [Id] = @SDG
UPDATE @Currencies SET [Name] = N'Dollar', [Name2] = N'دولار' WHERE [Id] = @USD
UPDATE @Currencies SET [Name] = N'Riyal', [Name2] = N'ريال' WHERE [Id] = @SAR
UPDATE @Currencies SET [Name] = N'Dirham', [Name2] = N'درهم' WHERE [Id] = @AED

EXEC [api].[Currencies__Save]
	@Entities = @Currencies,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Currencies Updating: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

-- Add exchange Rates
INSERT INTO @ExchangeRates([Index],[CurrencyId], [ValidAsOf], [AmountInCurrency], [AmountInFunctional]) VAlUES
(0,N'USD', N'2019-12-31', 1,45.1463),
(1,N'SAR', N'2019-12-31', 1,12.039),
(2,N'AED', N'2019-12-31', 1,12.2931);

EXEC [api].[ExchangeRates__Save]
	@Entities = @ExchangeRates,
	@ValidationErrorsJson = @ValidationErrorsJson
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Exchange Rates Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;