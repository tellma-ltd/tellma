-- Activate Currencies in Use
INSERT INTO @IndexedCurrencyIds
([Index],	[Id]) VALUES
(0,			@ETB),
(1,			@USD);
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

UPDATE @Currencies SET [Name] = N'Dollar' WHERE [Id] = @USD

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
(0,N'USD', N'2020-07-07', 1,35.14003),
(1,N'USD', N'2020-07-27', 1,35.2289);

EXEC [api].[ExchangeRates__Save]
	@Entities = @ExchangeRates,
	@ValidationErrorsJson = @ValidationErrorsJson
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Exchange Rates Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;