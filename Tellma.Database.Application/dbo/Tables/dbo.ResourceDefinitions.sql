CREATE TABLE [dbo].[ResourceDefinitions]
(
	[Id]								INT				CONSTRAINT [PK_ResourceDefinitions] PRIMARY KEY IDENTITY,
	[Code]								NVARCHAR (255)	NOT NULL CONSTRAINT [UX_ResourceDefinitions] UNIQUE,
	[TitleSingular]						NVARCHAR (100),
	[TitleSingular2]					NVARCHAR (100),
	[TitleSingular3]					NVARCHAR (100),
	[TitlePlural]						NVARCHAR (100),
	[TitlePlural2]						NVARCHAR (100),
	[TitlePlural3]						NVARCHAR (100),
	-- TODO Make NOT NULL
	[ResourceDefinitionType]			NVARCHAR (255) NOT NULL
	CONSTRAINT [CK_ResourceDefinitions__ResourceDefinitionType] CHECK ([ResourceDefinitionType] IN (
		N'PropertyPlantAndEquipment',
		N'InvestmentProperty',
		N'IntangibleAssetsOtherThanGoodwill',
		N'OtherFinancialAssets',
		N'BiologicalAssets',
		N'InventoriesTotal',
		N'TradeAndOtherReceivables',
		N'CashAndCashEquivalents',
		N'TradeAndOtherPayables',
		N'Provisions',
		N'OtherFinancialLiabilities',
		N'Miscellaneous' -- Tasks, etc.
	)),

	-----Resource Properties Common with Contracts
	[CurrencyVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'Required' CHECK ([CurrencyVisibility] IN (N'None', N'Optional', N'Required')),
	[CenterVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([CenterVisibility] IN (N'None', N'Optional', N'Required')),
	[ImageVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ImageVisibility] IN (N'None', N'Optional', N'Required')),
	[DescriptionVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([DescriptionVisibility] IN (N'None', N'Optional', N'Required')),
	[LocationVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([LocationVisibility] IN (N'None', N'Optional', N'Required')),

	[FromDateVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([FromDateVisibility] IN (N'None', N'Optional', N'Required')),
	[FromDateLabel]						NVARCHAR (50),
	[FromDateLabel2]					NVARCHAR (50),
	[FromDateLabel3]					NVARCHAR (50),

	[ToDateVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ToDateVisibility] IN (N'None', N'Optional', N'Required')),
	[ToDateLabel]						NVARCHAR (50),
	[ToDateLabel2]						NVARCHAR (50),
	[ToDateLabel3]						NVARCHAR (50),

	[Decimal1Visibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Decimal1Visibility] IN (N'None', N'Optional', N'Required')),
	[Decimal1Label]						NVARCHAR (50),
	[Decimal1Label2]					NVARCHAR (50),
	[Decimal1Label3]					NVARCHAR (50),		

	[Decimal2Visibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Decimal2Visibility] IN (N'None', N'Optional', N'Required')),
	[Decimal2Label]						NVARCHAR (50),
	[Decimal2Label2]					NVARCHAR (50),
	[Decimal2Label3]					NVARCHAR (50),		

	[Int1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Int1Visibility] IN (N'None', N'Optional', N'Required')),
	[Int1Label]							NVARCHAR (50),
	[Int1Label2]						NVARCHAR (50),
	[Int1Label3]						NVARCHAR (50),		

	[Int2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Int2Visibility] IN (N'None', N'Optional', N'Required')),
	[Int2Label]							NVARCHAR (50),
	[Int2Label2]						NVARCHAR (50),
	[Int2Label3]						NVARCHAR (50),		

	[Lookup1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup1Visibility] IN (N'None', N'Required', N'Optional')),
	[Lookup1DefinitionId]				INT				CONSTRAINT [FK_ResourceDefinitions__Lookup1DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup1Label]						NVARCHAR (50),
	[Lookup1Label2]						NVARCHAR (50),
	[Lookup1Label3]						NVARCHAR (50),

	[Lookup2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup2Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup2DefinitionId]				INT				CONSTRAINT [FK_ResourceDefinitions__Lookup2DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),	
	[Lookup2Label]						NVARCHAR (50),
	[Lookup2Label2]						NVARCHAR (50),
	[Lookup2Label3]						NVARCHAR (50),

	[Lookup3Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup3Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup3DefinitionId]				INT				CONSTRAINT [FK_ResourceDefinitions__Lookup3DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup3Label]						NVARCHAR (50),
	[Lookup3Label2]						NVARCHAR (50),
	[Lookup3Label3]						NVARCHAR (50),

	[Lookup4Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup4Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup4DefinitionId]				INT				CONSTRAINT [FK_ResourceDefinitions__Lookup4DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Lookup4Label]						NVARCHAR (50),
	[Lookup4Label2]						NVARCHAR (50),
	[Lookup4Label3]						NVARCHAR (50),

	[Text1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Text1Visibility] IN (N'None', N'Optional', N'Required')),
	[Text1Label]						NVARCHAR (50),
	[Text1Label2]						NVARCHAR (50),
	[Text1Label3]						NVARCHAR (50),		

	[Text2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Text2Visibility] IN (N'None', N'Optional', N'Required')),
	[Text2Label]						NVARCHAR (50),
	[Text2Label2]						NVARCHAR (50),
	[Text2Label3]						NVARCHAR (50),		
	
	[PreprocessScript]					NVARCHAR (MAX),
	[ValidateScript]					NVARCHAR (MAX),

	-----Properties applicable to resources only
	-- Resource properties
	[IdentifierVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([IdentifierVisibility] IN (N'None', N'Optional', N'Required')),
	[IdentifierLabel]					NVARCHAR (50), -- searchable, unique index
	[IdentifierLabel2]					NVARCHAR (50),
	[IdentifierLabel3]					NVARCHAR (50),
	[VatRateVisibility]					NVARCHAR (50)	DEFAULT N'None' CHECK ([VatRateVisibility] IN (N'None', N'Optional', N'Required')),-- TODO: Make required
	[DefaultVatRate]					DECIMAL (9,4)	CONSTRAINT [ResourceDefinitions__DefaultVatRate] CHECK ([DefaultVatRate] BETWEEN 0 AND 1),

	-- Inventory
	[ReorderLevelVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ReorderLevelVisibility] IN (N'None', N'Optional', N'Required')),
	[EconomicOrderQuantityVisibility]	NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([EconomicOrderQuantityVisibility] IN (N'None', N'Optional', N'Required')),
	[UnitCardinality]					NVARCHAR (50)	NOT NULL DEFAULT N'Single' CHECK ([UnitCardinality] IN (N'None', N'Single', N'Multiple')),
	[DefaultUnitId]						INT				CONSTRAINT [ResourceDefinitions__DefaultUnitId] REFERENCES dbo.Units([Id]),
	[UnitMassVisibility]				NVARCHAR (50)	DEFAULT N'None'  CHECK ([UnitMassVisibility] IN (N'None', N'Optional', N'Required')),-- make it required
	[DefaultUnitMassUnitId]				INT				CONSTRAINT [ResourceDefinitions__DefaultUnitMassUnitId] REFERENCES dbo.Units([Id]),

	-- Financial instruments
	[MonetaryValueVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([MonetaryValueVisibility] IN (N'None', N'Optional', N'Required')),
	[ParticipantVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ParticipantVisibility] IN (N'None', N'Optional', N'Required')),
	[ParticipantDefinitionId]			INT				CONSTRAINT [FK_ResourceDefinitions__ParticipantDefinitionId] REFERENCES dbo.RelationDefinitions([Id]),

	[Resource1Visibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Resource1Visibility] IN (N'None', N'Required', N'Optional')),
	[Resource1DefinitionId]				INT				CONSTRAINT [FK_ResourceDefinitions__Resource1DefinitionId] REFERENCES dbo.ResourceDefinitions([Id]),
	[Resource1Label]					NVARCHAR (50),
	[Resource1Label2]					NVARCHAR (50),
	[Resource1Label3]					NVARCHAR (50),

	[State]								NVARCHAR (50)	NOT NULL DEFAULT N'Hidden' CHECK([State] IN (N'Hidden', N'Visible', N'Archived')),	-- Visible, Readonly (Phased Out)
	[MainMenuIcon]						NVARCHAR (50),
	[MainMenuSection]					NVARCHAR (50),			-- IF Null, it does not show on the main menu
	[MainMenuSortKey]					DECIMAL (9,4),
	
	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_ResourceDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[ResourceDefinitionsHistory]));
GO;