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
