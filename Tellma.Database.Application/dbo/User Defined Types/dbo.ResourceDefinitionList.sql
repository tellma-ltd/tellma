CREATE TYPE [dbo].[ResourceDefinitionList] AS TABLE (
	[Index]								INT				PRIMARY KEY,
	[Id]								INT				NOT NULL DEFAULT 0,
	[Code]								NVARCHAR (255),
	[TitleSingular]						NVARCHAR (100),
	[TitleSingular2]					NVARCHAR (100),
	[TitleSingular3]					NVARCHAR (100),
	[TitlePlural]						NVARCHAR (100),
	[TitlePlural2]						NVARCHAR (100),
	[TitlePlural3]						NVARCHAR (100),
	[ResourceDefinitionType]			NVARCHAR (255),

	-----Resource Properties Common with Contracts
	[CurrencyVisibility]				NVARCHAR (50),
	[CenterVisibility]					NVARCHAR (50),
	[ImageVisibility]					NVARCHAR (50),
	[DescriptionVisibility]				NVARCHAR (50),
	[LocationVisibility]				NVARCHAR (50),

	[FromDateLabel]						NVARCHAR (50),
	[FromDateLabel2]					NVARCHAR (50),
	[FromDateLabel3]					NVARCHAR (50),		
	[FromDateVisibility]				NVARCHAR (50),
	[ToDateLabel]						NVARCHAR (50),
	[ToDateLabel2]						NVARCHAR (50),
	[ToDateLabel3]						NVARCHAR (50),
	[ToDateVisibility]					NVARCHAR (50),

	[Date1Label]						NVARCHAR (50),
	[Date1Label2]						NVARCHAR (50),
	[Date1Label3]						NVARCHAR (50),
	[Date1Visibility]					NVARCHAR (50),

	[Date2Label]						NVARCHAR (50),
	[Date2Label2]						NVARCHAR (50),
	[Date2Label3]						NVARCHAR (50),
	[Date2Visibility]					NVARCHAR (50),

	[Date3Label]						NVARCHAR (50),
	[Date3Label2]						NVARCHAR (50),
	[Date3Label3]						NVARCHAR (50),
	[Date3Visibility]					NVARCHAR (50),

	[Date4Label]						NVARCHAR (50),
	[Date4Label2]						NVARCHAR (50),
	[Date4Label3]						NVARCHAR (50),
	[Date4Visibility]					NVARCHAR (50),

	[Decimal1Label]						NVARCHAR (50),
	[Decimal1Label2]					NVARCHAR (50),
	[Decimal1Label3]					NVARCHAR (50),		
	[Decimal1Visibility]				NVARCHAR (50),

	[Decimal2Label]						NVARCHAR (50),
	[Decimal2Label2]					NVARCHAR (50),
	[Decimal2Label3]					NVARCHAR (50),		
	[Decimal2Visibility]				NVARCHAR (50),

	[Decimal3Label]						NVARCHAR (50),
	[Decimal3Label2]					NVARCHAR (50),
	[Decimal3Label3]					NVARCHAR (50),		
	[Decimal3Visibility]				NVARCHAR (50),

	[Decimal4Label]						NVARCHAR (50),
	[Decimal4Label2]					NVARCHAR (50),
	[Decimal4Label3]					NVARCHAR (50),		
	[Decimal4Visibility]				NVARCHAR (50),

	[Int1Label]							NVARCHAR (50),
	[Int1Label2]						NVARCHAR (50),
	[Int1Label3]						NVARCHAR (50),		
	[Int1Visibility]					NVARCHAR (50),

	[Int2Label]							NVARCHAR (50),
	[Int2Label2]						NVARCHAR (50),
	[Int2Label3]						NVARCHAR (50),		
	[Int2Visibility]					NVARCHAR (50),

	[Lookup1Label]						NVARCHAR (50),
	[Lookup1Label2]						NVARCHAR (50),
	[Lookup1Label3]						NVARCHAR (50),
	[Lookup1Visibility]					NVARCHAR (50),
	[Lookup1DefinitionId]				INT,
	[Lookup2Label]						NVARCHAR (50),
	[Lookup2Label2]						NVARCHAR (50),
	[Lookup2Label3]						NVARCHAR (50),
	[Lookup2Visibility]					NVARCHAR (50),
	[Lookup2DefinitionId]				INT,
	[Lookup3Label]						NVARCHAR (50),
	[Lookup3Label2]						NVARCHAR (50),
	[Lookup3Label3]						NVARCHAR (50),
	[Lookup3Visibility]					NVARCHAR (50),
	[Lookup3DefinitionId]				INT,
	[Lookup4Label]						NVARCHAR (50),
	[Lookup4Label2]						NVARCHAR (50),
	[Lookup4Label3]						NVARCHAR (50),
	[Lookup4Visibility]					NVARCHAR (50),
	[Lookup4DefinitionId]				INT,

	[Text1Label]						NVARCHAR (50),
	[Text1Label2]						NVARCHAR (50),
	[Text1Label3]						NVARCHAR (50),		
	[Text1Visibility]					NVARCHAR (50),

	[Text2Label]						NVARCHAR (50),
	[Text2Label2]						NVARCHAR (50),
	[Text2Label3]						NVARCHAR (50),		
	[Text2Visibility]					NVARCHAR (50),
	
	[PreprocessScript]					NVARCHAR (MAX),
	[ValidateScript]					NVARCHAR (MAX),
	
	[IdentifierLabel]					NVARCHAR (50), -- searchable, unique index
	[IdentifierLabel2]					NVARCHAR (50),
	[IdentifierLabel3]					NVARCHAR (50),
	[IdentifierVisibility]				NVARCHAR (50),
	-----Properties applicable to resources only
	-- Resource properties

	[VatRateVisibility]					NVARCHAR (50),
	[DefaultVatRate]					DECIMAL (9,4),
	-- Inventory
	[ReorderLevelVisibility]			NVARCHAR (50),
	[EconomicOrderQuantityVisibility]	NVARCHAR (50),
	[UnitCardinality]					NVARCHAR (50),
	[DefaultUnitId]						INT,
	[UnitMassVisibility]				NVARCHAR (50),
	[DefaultUnitMassUnitId]				INT,

	-- Financial instruments
	[MonetaryValueVisibility]			NVARCHAR (50),

	[Agent1Label]						NVARCHAR (50),
	[Agent1Label2]						NVARCHAR (50),
	[Agent1Label3]						NVARCHAR (50),
	[Agent1Visibility]					NVARCHAR (50),
	[Agent1DefinitionId]				INT,

	[Agent2Label]						NVARCHAR (50),
	[Agent2Label2]						NVARCHAR (50),
	[Agent2Label3]						NVARCHAR (50),
	[Agent2Visibility]					NVARCHAR (50),
	[Agent2DefinitionId]				INT,


	[Resource1Label]					NVARCHAR (50),
	[Resource1Label2]					NVARCHAR (50),
	[Resource1Label3]					NVARCHAR (50),
	[Resource1Visibility]				NVARCHAR (50),
	[Resource1DefinitionIndex]			INT,
	[Resource1DefinitionId]				INT,

	[Resource2Label]					NVARCHAR (50),
	[Resource2Label2]					NVARCHAR (50),
	[Resource2Label3]					NVARCHAR (50),
	[Resource2Visibility]				NVARCHAR (50),
	[Resource2DefinitionIndex]			INT,
	[Resource2DefinitionId]				INT,
	[HasAttachments]					BIT,
	[AttachmentsCategoryDefinitionId]	INT,

	[MainMenuIcon]						NVARCHAR (50),
	[MainMenuSection]					NVARCHAR (50),
	[MainMenuSortKey]					DECIMAL (9,4)
);