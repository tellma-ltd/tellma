CREATE TABLE [dbo].[Settings] ( -- TODO: Make it wide table, up to 30,0000 columns
	-- General Settings
	[CreatedAt]								DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]							INT					NOT NULL CONSTRAINT [FK_Settings__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[CompanyName]							NVARCHAR (255), -- Full company name as it appears in government records
	[CompanyName2]							NVARCHAR (255),
	[CompanyName3]							NVARCHAR (255),

	[CustomFieldsJson]  					NVARCHAR (MAX),
		/*
            Version
            BuildingNumber
            Street
            Street2
            Street3
            SecondaryNumber
            District
            District2
            District3
            PostalCode
            City
            City2
            City3
		*/
	[CountryCode]							NVARCHAR(2),

	[ShortCompanyName]						NVARCHAR (255)		NOT NULL, -- Appears under the user's name and when browsing My Companies
	[ShortCompanyName2]						NVARCHAR (255),
	[ShortCompanyName3]						NVARCHAR (255),
	
	[PrimaryLanguageId]						NVARCHAR (5)		NOT NULL,
	[PrimaryLanguageSymbol]					NVARCHAR (5),
	[SecondaryLanguageId]					NVARCHAR (5),
	[SecondaryLanguageSymbol]				NVARCHAR (5),
	[TernaryLanguageId]						NVARCHAR (5),
	[TernaryLanguageSymbol]					NVARCHAR (5),
	[PrimaryCalendar]						NCHAR (2)			NOT NULL DEFAULT N'GC',
	[SecondaryCalendar]						NCHAR (2),
	[DateFormat]							NVARCHAR (50)		NOT NULL DEFAULT N'yyyy-MM-dd',
	[TimeFormat]							NVARCHAR (50)		NOT NULL DEFAULT N'HH:mm:ss',
	[BrandColor]							NCHAR (7),
	[SupportEmails]							NVARCHAR (4000),
	[SmsEnabled]							BIT					NOT NULL DEFAULT 0, -- SMS is expensive, this value is only editable from Tellma's admin console
	
	-- Versions
	[DefinitionsVersion]					UNIQUEIDENTIFIER	NOT NULL DEFAULT NEWID(),
	[SettingsVersion]						UNIQUEIDENTIFIER	NOT NULL DEFAULT NEWID(),
	[SchedulesVersion]						UNIQUEIDENTIFIER	NOT NULL DEFAULT NEWID(),
	[GeneralModifiedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[GeneralModifiedById]					INT					NOT NULL CONSTRAINT [FK_Settings__GeneralModifiedById] REFERENCES [dbo].[Users] ([Id]),

	-- Financial Settings
	[FunctionalCurrencyId]					NCHAR(3)			NOT NULL CONSTRAINT [FK_Settings__FunctionalCurrencyId] REFERENCES [dbo].[Currencies]([Id]),
	[TaxIdentificationNumber]				NVARCHAR (50)		NULL,
	[FirstDayOfPeriod]						TINYINT				NOT NULL DEFAULT 1,
	[ArchiveDate]							DATE				NOT NULL DEFAULT ('1980.01.01'),
	[FinancialModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[FinancialModifiedById]					INT					NULL CONSTRAINT [FK_Settings__FinancialModifiedById] REFERENCES [dbo].[Users] ([Id]),
	[ReferenceSourceAgentDefinitionCodes]NVARCHAR (255) NOT NULL DEFAULT N'CashMachine,CashSaleVoucher,CreditSaleVoucher',

	-- Zatca
	[ZatcaEncryptedPrivateKey]				NVARCHAR(MAX),
	[ZatcaEncryptedSecret]					NVARCHAR(MAX),
	[ZatcaEncryptedCsid]					NVARCHAR(MAX),
	[ZatcaEncryptionKeyIndex]				INT					NOT NULL DEFAULT 0,
	[ZatcaUseSandbox]						BIT					NOT NULL DEFAULT 1,

);
--	IFRS [810000]
	--[NameOfReportingEntityOrOtherMeansOfIdentification]	NVARCHAR (255),
	--[DomicileOfEntity]				NVARCHAR (255),
	--[DomicileOfEntity2]				NVARCHAR (255),
	--[DomicileOfEntity3]				NVARCHAR (255),
	--[LegalFormOfEntity]				NVARCHAR (255),
	--[LegalFormOfEntity2]				NVARCHAR (255),
	--[LegalFormOfEntity3]				NVARCHAR (255),	
	--[CountryOfIncorporation]			NVARCHAR (255),
	--[CountryOfIncorporation2]			NVARCHAR (255),
	--[CountryOfIncorporation3]			NVARCHAR (255),
	--[AddressOfRegisteredOffice]		NVARCHAR (255),
	--[AddressOfRegisteredOffice2]		NVARCHAR (255),
	--[AddressOfRegisteredOffice3]		NVARCHAR (255),
	--[PrincipalPlaceOfBusiness]		NVARCHAR (255),
	--[PrincipalPlaceOfBusiness2]		NVARCHAR (255),
	--[PrincipalPlaceOfBusiness3]		NVARCHAR (255),
	--[NatureOfOperations]				NVARCHAR (255),
	--[NatureOfOperations2]				NVARCHAR (255),
	--[NatureOfOperations3]				NVARCHAR (255),
	--[NameOfParentEntity]				NVARCHAR (255),
	--[NameOfParentEntity2]				NVARCHAR (255),
	--[NameOfParentEntity3]				NVARCHAR (255),
	--[NameOfUltimateParentOfGroup]		NVARCHAR (255),
	--[NameOfUltimateParentOfGroup2]	NVARCHAR (255),
	--[NameOfUltimateParentOfGroup3]	NVARCHAR (255)