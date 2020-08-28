CREATE FUNCTION [map].[ResourceDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[Code],
		[TitleSingular]	,
		[TitleSingular2],
		[TitleSingular3],
		[TitlePlural],
		[TitlePlural2],
		[TitlePlural3],

		[ResourceDefinitionType],

		[CurrencyVisibility],
		[CenterVisibility],
		[ImageVisibility],
		[DescriptionVisibility],
		[LocationVisibility],

		[FromDateVisibility],
		[FromDateLabel]	,
		[FromDateLabel2],
		[FromDateLabel3]	,

		[ToDateVisibility]	,
		[ToDateLabel]		,
		[ToDateLabel2],
		[ToDateLabel3]	,

		[Decimal1Visibility],
		[Decimal1Label]	,
		[Decimal1Label2],
		[Decimal1Label3],

		[Decimal2Visibility],
		[Decimal2Label]	,
		[Decimal2Label2],
		[Decimal2Label3],	

		[Int1Visibility],
		[Int1Label],
		[Int1Label2],
		[Int1Label3],

		[Int2Visibility],
		[Int2Label]	,
		[Int2Label2],
		[Int2Label3],

		[Lookup1Visibility],
		[Lookup1DefinitionId],
		[Lookup1Label],
		[Lookup1Label2],
		[Lookup1Label3],

		[Lookup2Visibility]	,
		[Lookup2DefinitionId],	
		[Lookup2Label],
		[Lookup2Label2]	,
		[Lookup2Label3]	,

		[Lookup3Visibility]	,
		[Lookup3DefinitionId],
		[Lookup3Label],
		[Lookup3Label2],
		[Lookup3Label3]	,

		[Lookup4Visibility]	,
		[Lookup4DefinitionId]	,
		[Lookup4Label]	,
		[Lookup4Label2]	,
		[Lookup4Label3]	,

		[Text1Visibility]	,
		[Text1Label],
		[Text1Label2],
		[Text1Label3],

		[Text2Visibility],
		[Text2Label],
		[Text2Label2],
		[Text2Label3],	

		[Script],

		-- Resource properties
		[IdentifierVisibility],
		[IdentifierLabel],
		[IdentifierLabel2],
		[IdentifierLabel3],
		[VatRateVisibility],
		[DefaultVatRate],

		-- Inventory
		[ReorderLevelVisibility],
		[EconomicOrderQuantityVisibility],
		[UnitCardinality],
		[DefaultUnitId],
		[UnitMassVisibility],
		[DefaultUnitMassUnitId],

		-- Financial instruments
		[MonetaryValueVisibility],
		[ParticipantVisibility]	,
		[ParticipantDefinitionId],

		[State]	,
		[MainMenuIcon],
		[MainMenuSection],
		[MainMenuSortKey],
	
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[ResourceDefinitions]
);
