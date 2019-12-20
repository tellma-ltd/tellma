IF NOT EXISTS(SELECT * FROM dbo.Currencies WHERE [Id] = @FunctionalCurrencyId)
BEGIN
	DECLARE @IsoCurrencies dbo.CurrencyList
	INSERT INTO @IsoCurrencies([Index],
	[Id],	[Name],			[Name2],			[Description],			[Description2],		[E]) VALUES
(0, N'USD', N'USD',			N'دولار',			N'United States Dollar',N'دولار أمريكي',		2),
(1, N'ETB', N'Birr',		N'بر',				N'Ethiopian Birr',		N'بر أثيوبي',		2),
(2, N'GBP', N'Pounds',		N'جنيه',			N'Sterling Pound',		N'جنيه استرليني',	2),
(3, N'AED', N'Dirhams',		N'درهم',			N'Emirates Dirham',		N'درهم إماراتي',	2),
(4, N'JPY', N'Yen',			N'ين',				N'Japanese Yen',		N'ين ياباني',		2);

	DECLARE @FunctionalCurrencies dbo.CurrencyList; -- actually, it is only one
	INSERT INTO @FunctionalCurrencies
	SELECT * FROM @IsoCurrencies WHERE [Id] = @FunctionalCurrencyId;

	EXEC [api].Currencies__Save
		@Entities = @FunctionalCurrencies,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Currencies: Inserting'
		GOTO Err_Label;
	END;						
	EXEC master.sys.sp_set_session_context 'FunctionalCurrencyId', @FunctionalCurrencyId;
END