CREATE TABLE [dbo].[Settings] ( -- TODO: Make it wide table, up to 30,0000 columns
--	IFRS [110000]
	[ShortCompanyName]				NVARCHAR (255)		NOT NULL,
	[ShortCompanyName2]				NVARCHAR (255),
	[ShortCompanyName3]				NVARCHAR (255),

	[FunctionalCurrencyId]			NCHAR(3)			CONSTRAINT [FK_Settings__CurrencyId] REFERENCES dbo.Currencies([Id]),
--	IFRS [810000]
	[DomicileOfEntity]				NVARCHAR (255),
	[DomicileOfEntity2]				NVARCHAR (255),
	[DomicileOfEntity3]				NVARCHAR (255),
	[LegalFormOfEntity]				NVARCHAR (255),
	[LegalFormOfEntity2]			NVARCHAR (255),
	[LegalFormOfEntity3]			NVARCHAR (255),	
	[CountryOfIncorporation]		NVARCHAR (255),
	[CountryOfIncorporation2]		NVARCHAR (255),
	[CountryOfIncorporation3]		NVARCHAR (255),
	[AddressOfRegisteredOffice]		NVARCHAR (255),
	[AddressOfRegisteredOffice2]	NVARCHAR (255),
	[AddressOfRegisteredOffice3]	NVARCHAR (255),
	[PrincipalPlaceOfBusiness]		NVARCHAR (255),
	[PrincipalPlaceOfBusiness2]		NVARCHAR (255),
	[PrincipalPlaceOfBusiness3]		NVARCHAR (255),
	[NatureOfOperations]			NVARCHAR (255),
	[NatureOfOperations2]			NVARCHAR (255),
	[NatureOfOperations3]			NVARCHAR (255),
	[NameOfParentEntity]			NVARCHAR (255),
	[NameOfParentEntity2]			NVARCHAR (255),
	[NameOfParentEntity3]			NVARCHAR (255),
	[NameOfUltimateParentOfGroup]	NVARCHAR (255),
	[NameOfUltimateParentOfGroup2]	NVARCHAR (255),
	[NameOfUltimateParentOfGroup3]	NVARCHAR (255),
	--
	[PrimaryLanguageId]				NVARCHAR (5)		NOT NULL,
	[PrimaryLanguageSymbol]			NVARCHAR (5),
	[SecondaryLanguageId]			NVARCHAR (5),
	[SecondaryLanguageSymbol]		NVARCHAR (5),
	[TernaryLanguageId]				NVARCHAR (5),
	[TernaryLanguageSymbol]			NVARCHAR (5),
	[BrandColor]					NVARCHAR (255),
	[DefinitionsVersion]			UNIQUEIDENTIFIER	NOT NULL,
	[SettingsVersion]				UNIQUEIDENTIFIER	NOT NULL,




	-- The date before which data is frozen.
	[ArchiveDate]					DATE				NOT NULL DEFAULT ('1900.01.01'),

	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Settings__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Settings__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);