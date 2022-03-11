IF EXISTS (SELECT 1 FROM [dbo].[Settings])
	UPDATE [dbo].[Settings] SET
		-- General Settings
		[ShortCompanyName] = @ShortCompanyName,
		[ShortCompanyName2] = @ShortCompanyName2,
		[ShortCompanyName3] = @ShortCompanyName3,
		[PrimaryLanguageId] = @PrimaryLanguageId,
		[PrimaryLanguageSymbol] = @PrimaryLanguageSymbol,
		[SecondaryLanguageId] = @SecondaryLanguageId,
		[SecondaryLanguageSymbol] = @SecondaryLanguageSymbol,
		[TernaryLanguageId] = @TernaryLanguageId,
		[TernaryLanguageSymbol] = @TernaryLanguageSymbol,
		[BrandColor] = @BrandColor
ELSE

	INSERT INTO [dbo].[Settings] (
		[CreatedById],

		-- General Settings
		[ShortCompanyName],
		[ShortCompanyName2],
		[ShortCompanyName3],
		[PrimaryLanguageId],
		[PrimaryLanguageSymbol],
		[SecondaryLanguageId],
		[SecondaryLanguageSymbol],
		[TernaryLanguageId],
		[TernaryLanguageSymbol],
		[BrandColor],
		[GeneralModifiedById],
		
		-- Financial Settings
		[FunctionalCurrencyId],
		[FinancialModifiedById]
	)
	VALUES(
		@AdminUserId,

		@ShortCompanyName,
		@ShortCompanyName2,
		@ShortCompanyName3,
		@PrimaryLanguageId,
		@PrimaryLanguageSymbol,
		@SecondaryLanguageId,
		@SecondaryLanguageSymbol,
		@TernaryLanguageId,
		@TernaryLanguageSymbol,
		@BrandColor, 
		@AdminUserId, 

		@FunctionalCurrencyId,
		@AdminUserId
	);