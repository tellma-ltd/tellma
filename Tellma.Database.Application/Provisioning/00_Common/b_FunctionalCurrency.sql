IF NOT EXISTS(SELECT * FROM dbo.Currencies WHERE [Id] = @FunctionalCurrencyId)
BEGIN
	DECLARE @FunctionalCurrencies dbo.CurrencyList; -- actually, it is only one

	IF @DB = N'100' -- ACME, USD, en/ar/zh playground
	BEGIN
		INSERT INTO @FunctionalCurrencies
		([Id],	[Name],		[Name2],	[Name3],	[Description],			[Description2],	[Description3],	[E]) VALUES
		(N'USD', N'Dollar',	N'دولار',	N'美元',		N'United States Dollar',N'دولار أمريكي',	N'美国美元',		2);
	END
	ELSE IF @DB = N'101' -- Banan SD, USD, en
	BEGIN
		INSERT INTO @FunctionalCurrencies(
		[Id],	[Name],		[Description],				[E]) VALUES
		(N'USD', N'Dollar',	N'United States Dollar',	2);
	END
	ELSE IF @DB = N'102' -- Banan ET, ETB, en
	BEGIN
		INSERT INTO @FunctionalCurrencies
		([Id],	[Name],		[Description],		[E]) VALUES
		(N'ETB', N'Birr',	N'Ethiopian Birr',	2);
	END
	ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh car service
	BEGIN
		INSERT INTO @FunctionalCurrencies
		([Id],	[Name],		[Name2],	[Description],		[Description2],		[E]) VALUES
		(N'ETB', N'Birr',	N'比尔',		N'Ethiopian Birr',	N'埃塞俄比亚比尔',	2);
	END
	ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am manyfacturing and sales
	BEGIN
		INSERT INTO @FunctionalCurrencies
		([Id],	[Name],		[Name2],[Description],		[Description2],	[E]) VALUES
		(N'ETB', N'Birr',	N'ብር',	N'Ethiopian Birr',	N'የኢትዮጵያ ብር',	2);
	END
	ELSE IF @DB = N'105' -- Simpex, SAR, en/ar trading
	BEGIN
		INSERT INTO @FunctionalCurrencies
		([Id],	[Name],		[Name2],[Description],	[Description2],	[E]) VALUES
		(N'SAR', N'Riyal',	N'ريال',	N'Saudi Riyal',	N'ريال سعودي',		2);
	END

	DELETE FROM @FunctionalCurrencies WHERE [Id] IN (SELECT [Id] FROM dbo.[Currencies])
	EXEC [api].Currencies__Save
		@Entities = @FunctionalCurrencies,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Currencies: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;						
END