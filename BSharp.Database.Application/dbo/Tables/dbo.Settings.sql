CREATE TABLE [dbo].[Settings] ( -- TODO: Make it wide table, up to 30,0000 columns
	[ShortCompanyName]			NVARCHAR (255) NOT NULL,
	[ShortCompanyName2]			NVARCHAR (255) NULL,
	[ShortCompanyName3]			NVARCHAR (255) NULL,
	[PrimaryLanguageId]			NVARCHAR (5) NOT NULL,
	[PrimaryLanguageSymbol]		NVARCHAR (5) NULL,
	[SecondaryLanguageId]		NVARCHAR (5) NULL,
	[SecondaryLanguageSymbol]	NVARCHAR (5) NULL,
	[TernaryLanguageId]			NVARCHAR (5) NULL,
	[TernaryLanguageSymbol]		NVARCHAR (5) NULL,
	[BrandColor]				NVARCHAR (255) NULL,
	[DefinitionsVersion]		UNIQUEIDENTIFIER NOT NULL,
	[SettingsVersion]			UNIQUEIDENTIFIER NOT NULL,

	[FunctionalCurrencyId]		NCHAR(3)			CONSTRAINT [FK_Settings__CurrencyId] REFERENCES dbo.Currencies([Id]),
	-- The date before which data is frozen.
	[ArchiveDate]				DATE				NOT NULL DEFAULT ('1900.01.01'),
	[ResourceLookup1Label]		NVARCHAR (50),
	[ResourceLookup1Label2]		NVARCHAR (50),
	[ResourceLookup1Label3]		NVARCHAR (50),
	[ResourceLookup1sLabel]		NVARCHAR (50),
	[ResourceLookup1sLabel2]	NVARCHAR (50),
	[ResourceLookup1sLabel3]	NVARCHAR (50),

	[ResourceLookup2Label]		NVARCHAR (50),
	[ResourceLookup3Label]		NVARCHAR (50),

	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Settings__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Settings__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);