CREATE TYPE [dbo].[ResourceDefinitionList] AS TABLE (
	[Index]								INT				PRIMARY KEY,
	[Id]								INT				NOT NULL DEFAULT 0,
	[Code]								NVARCHAR (50)	NOT NULL UNIQUE,
	[TitleSingular]						NVARCHAR (255),
	[TitleSingular2]					NVARCHAR (255),
	[TitleSingular3]					NVARCHAR (255),
	[TitlePlural]						NVARCHAR (255),
	[TitlePlural2]						NVARCHAR (255),
	[TitlePlural3]						NVARCHAR (255),
	-----Resource Properties Common with Contracts
	[CurrencyVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([CurrencyVisibility] IN (N'None', N'Optional', N'Required')),
	[CenterVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([CenterVisibility] IN (N'None', N'Optional', N'Required')),
	[ImageVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ImageVisibility] IN (N'None', N'Optional', N'Required')),
	[DescriptionVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([DescriptionVisibility] IN (N'None', N'Optional', N'Required')),
	[LocationVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([LocationVisibility] IN (N'None', N'Optional', N'Required')),

	-- need to refactor to fromDate and toDate
	[FromDateLabel]						NVARCHAR (50),
	[FromDateLabel2]					NVARCHAR (50),
	[FromDateLabel3]					NVARCHAR (50),		
	[FromDateVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([FromDateVisibility] IN (N'None', N'Optional', N'Required')),
	[ToDateLabel]						NVARCHAR (50),
	[ToDateLabel2]						NVARCHAR (50),
	[ToDateLabel3]						NVARCHAR (50),
	[ToDateVisibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ToDateVisibility] IN (N'None', N'Optional', N'Required')),

	[Decimal1Label]						NVARCHAR (50),
	[Decimal1Label2]					NVARCHAR (50),
	[Decimal1Label3]					NVARCHAR (50),		
	[Decimal1Visibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Decimal1Visibility] IN (N'None', N'Optional', N'Required')),

	[Decimal2Label]						NVARCHAR (50),
	[Decimal2Label2]					NVARCHAR (50),
	[Decimal2Label3]					NVARCHAR (50),		
	[Decimal2Visibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Decimal2Visibility] IN (N'None', N'Optional', N'Required')),

	[Int1Label]							NVARCHAR (50),
	[Int1Label2]						NVARCHAR (50),
	[Int1Label3]						NVARCHAR (50),		
	[Int1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Int1Visibility] IN (N'None', N'Optional', N'Required')),

	[Int2Label]							NVARCHAR (50),
	[Int2Label2]						NVARCHAR (50),
	[Int2Label3]						NVARCHAR (50),		
	[Int2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Int2Visibility] IN (N'None', N'Optional', N'Required')),

	[Lookup1Label]						NVARCHAR (50),
	[Lookup1Label2]						NVARCHAR (50),
	[Lookup1Label3]						NVARCHAR (50),
	[Lookup1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup1Visibility] IN (N'None', N'Required', N'Optional')),
	[Lookup1DefinitionId]				INT,
	[Lookup2Label]						NVARCHAR (50),
	[Lookup2Label2]						NVARCHAR (50),
	[Lookup2Label3]						NVARCHAR (50),
	[Lookup2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup2Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup2DefinitionId]				INT,
	[Lookup3Label]						NVARCHAR (50),
	[Lookup3Label2]						NVARCHAR (50),
	[Lookup3Label3]						NVARCHAR (50),
	[Lookup3Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup3Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup3DefinitionId]				INT,
	[Lookup4Label]						NVARCHAR (50),
	[Lookup4Label2]						NVARCHAR (50),
	[Lookup4Label3]						NVARCHAR (50),
	[Lookup4Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Lookup4Visibility] IN (N'None', N'Optional', N'Required')),
	[Lookup4DefinitionId]				INT,

	[Text1Label]						NVARCHAR (50),
	[Text1Label2]						NVARCHAR (50),
	[Text1Label3]						NVARCHAR (50),		
	[Text1Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Text1Visibility] IN (N'None', N'Optional', N'Required')),

	[Text2Label]						NVARCHAR (50),
	[Text2Label2]						NVARCHAR (50),
	[Text2Label3]						NVARCHAR (50),		
	[Text2Visibility]					NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([Text2Visibility] IN (N'None', N'Optional', N'Required')),

	[Script]							NVARCHAR (MAX),
	-----Properties applicable to resources only
	-- Resource properties
	[IdentifierLabel]					NVARCHAR (50), -- searchable, unique index
	[IdentifierLabel2]					NVARCHAR (50),
	[IdentifierLabel3]					NVARCHAR (50),
	[IdentifierVisibility]				NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([IdentifierVisibility] IN (N'None', N'Optional', N'Required')),
		-- PPE
	[ResidualMonetaryValueVisibility]	NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ResidualMonetaryValueVisibility] IN (N'None', N'Optional', N'Required')),
	[ResidualValueVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ResidualValueVisibility] IN (N'None', N'Optional', N'Required')),
	-- Inventory
	[ReorderLevelVisibility]			NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([ReorderLevelVisibility] IN (N'None', N'Optional', N'Required')),
	[EconomicOrderQuantityVisibility]	NVARCHAR (50)	NOT NULL DEFAULT N'None' CHECK ([EconomicOrderQuantityVisibility] IN (N'None', N'Optional', N'Required')),
	[UnitCardinality]					NVARCHAR (50)	NOT NULL DEFAULT N'Single' CHECK ([UnitCardinality] IN (N'None', N'Single', N'Multiple')),
	--
	[MainMenuIcon]						NVARCHAR (50),
	[MainMenuSection]					NVARCHAR (50),			-- IF Null, it does not show on the main menu
	[MainMenuSortKey]					DECIMAL (9,4)
);