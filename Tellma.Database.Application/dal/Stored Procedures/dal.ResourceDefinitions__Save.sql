CREATE PROCEDURE [dal].[ResourceDefinitions__Save]
	@Entities [ResourceDefinitionList] READONLY,
	@ReturnIds BIT = 0
AS
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[ResourceDefinitions] AS t
		USING (
			SELECT 
				[Index], [Id],
				[Code],
				[TitleSingular],
				[TitleSingular2],
				[TitleSingular3],
				[TitlePlural],
				[TitlePlural2],
				[TitlePlural3],
				-----Contract properties common with resources
				--[CurrencyVisibility],
				[CenterVisibility],
				[ImageVisibility],
				[DescriptionVisibility],
				[LocationVisibility],

				[FromDateLabel],
				[FromDateLabel2],
				[FromDateLabel3],	
				[FromDateVisibility],
				[ToDateLabel],
				[ToDateLabel2],
				[ToDateLabel3],
				[ToDateVisibility],

				[Decimal1Label],
				[Decimal1Label2],
				[Decimal1Label3],	
				[Decimal1Visibility],

				[Decimal2Label],
				[Decimal2Label2],
				[Decimal2Label3],		
				[Decimal2Visibility],

				[Int1Label],
				[Int1Label2],
				[Int1Label3],	
				[Int1Visibility],

				[Int2Label],
				[Int2Label2],
				[Int2Label3],		
				[Int2Visibility],

				[Lookup1Label],
				[Lookup1Label2],
				[Lookup1Label3],
				[Lookup1Visibility],
				[Lookup1DefinitionId],
				[Lookup2Label],
				[Lookup2Label2],
				[Lookup2Label3],
				[Lookup2Visibility],
				[Lookup2DefinitionId],
				[Lookup3Label],
				[Lookup3Label2],
				[Lookup3Label3],
				[Lookup3Visibility],
				[Lookup3DefinitionId],
				[Lookup4Label],
				[Lookup4Label2],
				[Lookup4Label3],
				[Lookup4Visibility],
				[Lookup4DefinitionId],

				[Text1Label],
				[Text1Label2],
				[Text1Label3],	
				[Text1Visibility],

				[Text2Label],
				[Text2Label2],
				[Text2Label3],	
				[Text2Visibility],

				[Script],
				-----Properties applicable to resources only
				[IdentifierLabel],
				[IdentifierLabel2],
				[IdentifierLabel3],
				[IdentifierVisibility],
				[VatRateVisibility],
				[DefaultVatRate],
				-- Inventory
				[ReorderLevelVisibility],
				[EconomicOrderQuantityVisibility],
				[UnitCardinality],
				[DefaultUnitId],
				[UnitMassVisibility],
				[DefaultUnitMassUnitId],
				-- financial instruments
				[MonetaryValueVisibility],
				[ParticipantVisibility],
				[ParticipantDefinitionId],

				[MainMenuIcon],
				[MainMenuSection],
				[MainMenuSortKey]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Code]					= s.[Code],
				t.[TitleSingular]			= s.[TitleSingular],
				t.[TitleSingular2]			= s.[TitleSingular2],
				t.[TitleSingular3]			= s.[TitleSingular3],
				t.[TitlePlural]				= s.[TitlePlural],
				t.[TitlePlural2]			= s.[TitlePlural2],
				t.[TitlePlural3]			= s.[TitlePlural3],

				--t.[CurrencyVisibility]		= s.[CurrencyVisibility],
				t.[CenterVisibility]		= s.[CenterVisibility],
				t.[ImageVisibility]			= s.[ImageVisibility],
				t.[DescriptionVisibility]	= s.[DescriptionVisibility],
				t.[LocationVisibility]		= s.[LocationVisibility],

				t.[FromDateLabel]			= s.[FromDateLabel],
				t.[FromDateLabel2]			= s.[FromDateLabel2],
				t.[FromDateLabel3]			= s.[FromDateLabel3],	
				t.[FromDateVisibility]		= s.[FromDateVisibility],
				t.[ToDateLabel]				= s.[ToDateLabel],
				t.[ToDateLabel2]			= s.[ToDateLabel2],
				t.[ToDateLabel3]			= s.[ToDateLabel3],
				t.[ToDateVisibility]		= s.[ToDateVisibility],

				t.[Decimal1Label]			= s.[Decimal1Label],
				t.[Decimal1Label2]			= s.[Decimal1Label2],
				t.[Decimal1Label3]			= s.[Decimal1Label3],	
				t.[Decimal1Visibility]		= s.[Decimal1Visibility],

				t.[Decimal2Label]			= s.[Decimal2Label],
				t.[Decimal2Label2]			= s.[Decimal2Label2],
				t.[Decimal2Label3]			= s.[Decimal2Label3],		
				t.[Decimal2Visibility]		= s.[Decimal2Visibility],

				t.[Int1Label]				= s.[Int1Label],
				t.[Int1Label2]				= s.[Int1Label2],
				t.[Int1Label3]				= s.[Int1Label3],	
				t.[Int1Visibility]			= s.[Int1Visibility],

				t.[Int2Label]				= s.[Int2Label],
				t.[Int2Label2]				= s.[Int2Label2],
				t.[Int2Label3]				= s.[Int2Label3],		
				t.[Int2Visibility]			= s.[Int2Visibility],

				t.[Lookup1Label]			= s.[Lookup1Label],
				t.[Lookup1Label2]			= s.[Lookup1Label2],
				t.[Lookup1Label3]			= s.[Lookup1Label3],
				t.[Lookup1Visibility]		= s.[Lookup1Visibility],
				t.[Lookup1DefinitionId]		= s.[Lookup1DefinitionId],
				t.[Lookup2Label]			= s.[Lookup2Label],
				t.[Lookup2Label2]			= s.[Lookup2Label2],
				t.[Lookup2Label3]			= s.[Lookup2Label3],
				t.[Lookup2Visibility]		= s.[Lookup2Visibility],
				t.[Lookup2DefinitionId]		= s.[Lookup2DefinitionId],
				t.[Lookup3Label]			= s.[Lookup3Label],
				t.[Lookup3Label2]			= s.[Lookup3Label2],
				t.[Lookup3Label3]			= s.[Lookup3Label3],
				t.[Lookup3Visibility]		= s.[Lookup3Visibility],
				t.[Lookup3DefinitionId]		= s.[Lookup3DefinitionId],
				t.[Lookup4Label]			= s.[Lookup4Label],
				t.[Lookup4Label2]			= s.[Lookup4Label2],
				t.[Lookup4Label3]			= s.[Lookup4Label3],
				t.[Lookup4Visibility]		= s.[Lookup4Visibility],
				t.[Lookup4DefinitionId]		= s.[Lookup4DefinitionId],

				t.[Text1Label]				= s.[Text1Label],
				t.[Text1Label2]				= s.[Text1Label2],
				t.[Text1Label3]				= s.[Text1Label3],
				t.[Text1Visibility]			= s.[Text1Visibility],

				t.[Text2Label]				= s.[Text2Label],
				t.[Text2Label2]				= s.[Text2Label2],
				t.[Text2Label3]				= s.[Text2Label3],
				t.[Text2Visibility]			= s.[Text2Visibility],

				t.[Script]					= s.[Script],
				-----Properties applicable to resources only
				t.[IdentifierLabel]			= s.[IdentifierLabel],
				t.[IdentifierLabel2]		= s.[IdentifierLabel2],
				t.[IdentifierLabel3]		= s.[IdentifierLabel3],
				t.[IdentifierVisibility]	= s.[IdentifierVisibility],
				t.[VatRateVisibility]		= s.[VatRateVisibility],
				t.[DefaultVatRate]			= s.[DefaultVatRate],
				-- Inventory
				t.[ReorderLevelVisibility] = s.[ReorderLevelVisibility],
				t.[EconomicOrderQuantityVisibility]
											= s.[EconomicOrderQuantityVisibility],
				t.[UnitCardinality]			= s.[UnitCardinality],
				t.[DefaultUnitId]			= s.[DefaultUnitId],
				t.[UnitMassVisibility]		= s.[UnitMassVisibility],
				t.[DefaultUnitMassUnitId]	= s.[DefaultUnitMassUnitId],
				t.[MonetaryValueVisibility]	= s.[MonetaryValueVisibility],
				t.[ParticipantVisibility]	= s.[ParticipantVisibility],
				t.[ParticipantDefinitionId]	= s.[ParticipantDefinitionId],
				t.[MainMenuIcon]			= s.[MainMenuIcon],
				t.[MainMenuSection]			= s.[MainMenuSection],
				t.[MainMenuSortKey]			= s.[MainMenuSortKey],
				t.[SavedById]				= @UserId
		WHEN NOT MATCHED THEN
		INSERT ([Code],	[TitleSingular],	[TitleSingular2], [TitleSingular3],		[TitlePlural],	[TitlePlural2],		[TitlePlural3],
				--[CurrencyVisibility],
				[CenterVisibility],
				[ImageVisibility],
				[DescriptionVisibility],
				[LocationVisibility],

				[FromDateLabel],
				[FromDateLabel2],
				[FromDateLabel3],	
				[FromDateVisibility],
				[ToDateLabel],
				[ToDateLabel2],
				[ToDateLabel3],
				[ToDateVisibility],

				[Decimal1Label],
				[Decimal1Label2],
				[Decimal1Label3],	
				[Decimal1Visibility],

				[Decimal2Label],
				[Decimal2Label2],
				[Decimal2Label3],		
				[Decimal2Visibility],

				[Int1Label],
				[Int1Label2],
				[Int1Label3],	
				[Int1Visibility],

				[Int2Label],
				[Int2Label2],
				[Int2Label3],		
				[Int2Visibility],

				[Lookup1Label],
				[Lookup1Label2],
				[Lookup1Label3],
				[Lookup1Visibility],
				[Lookup1DefinitionId],
				[Lookup2Label],
				[Lookup2Label2],
				[Lookup2Label3],
				[Lookup2Visibility],
				[Lookup2DefinitionId],
				[Lookup3Label],
				[Lookup3Label2],
				[Lookup3Label3],
				[Lookup3Visibility],
				[Lookup3DefinitionId],
				[Lookup4Label],
				[Lookup4Label2],
				[Lookup4Label3],
				[Lookup4Visibility],
				[Lookup4DefinitionId],

				[Text1Label],
				[Text1Label2],
				[Text1Label3],	
				[Text1Visibility],

				[Text2Label],
				[Text2Label2],
				[Text2Label3],	
				[Text2Visibility],

				[Script],

				[IdentifierLabel],
				[IdentifierLabel2],
				[IdentifierLabel3],
				[IdentifierVisibility],
				[VatRateVisibility],
				[DefaultVatRate],
				-- Inventory
				[ReorderLevelVisibility],
				[EconomicOrderQuantityVisibility],
				[UnitCardinality],
				[DefaultUnitId],
				[UnitMassVisibility],
				[DefaultUnitMassUnitId],
				[MonetaryValueVisibility],
				[ParticipantVisibility],
				[ParticipantDefinitionId],

				[MainMenuIcon],
				[MainMenuSection],
				[MainMenuSortKey]
			
				)
			VALUES (s.[Code],	s.[TitleSingular],	s.[TitleSingular2], s.[TitleSingular3],		s.[TitlePlural],	s.[TitlePlural2],		s.[TitlePlural3],
				--s.[CurrencyVisibility],
				s.[CenterVisibility],
				s.[ImageVisibility],
				s.[DescriptionVisibility],
				s.[LocationVisibility],

				s.[FromDateLabel],
				s.[FromDateLabel2],
				s.[FromDateLabel3],	
				s.[FromDateVisibility],
				s.[ToDateLabel],
				s.[ToDateLabel2],
				s.[ToDateLabel3],
				s.[ToDateVisibility],

				s.[Decimal1Label],
				s.[Decimal1Label2],
				s.[Decimal1Label3],	
				s.[Decimal1Visibility],

				s.[Decimal2Label],
				s.[Decimal2Label2],
				s.[Decimal2Label3],		
				s.[Decimal2Visibility],

				s.[Int1Label],
				s.[Int1Label2],
				s.[Int1Label3],	
				s.[Int1Visibility],

				s.[Int2Label],
				s.[Int2Label2],
				s.[Int2Label3],		
				s.[Int2Visibility],

				s.[Lookup1Label],
				s.[Lookup1Label2],
				s.[Lookup1Label3],
				s.[Lookup1Visibility],
				s.[Lookup1DefinitionId],
				s.[Lookup2Label],
				s.[Lookup2Label2],
				s.[Lookup2Label3],
				s.[Lookup2Visibility],
				s.[Lookup2DefinitionId],
				s.[Lookup3Label],
				s.[Lookup3Label2],
				s.[Lookup3Label3],
				s.[Lookup3Visibility],
				s.[Lookup3DefinitionId],
				s.[Lookup4Label],
				s.[Lookup4Label2],
				s.[Lookup4Label3],
				s.[Lookup4Visibility],
				s.[Lookup4DefinitionId],

				s.[Text1Label],
				s.[Text1Label2],
				s.[Text1Label3],	
				s.[Text1Visibility],

				s.[Text2Label],
				s.[Text2Label2],
				s.[Text2Label3],	
				s.[Text2Visibility],

				s.[Script],

				s.[IdentifierLabel],
				s.[IdentifierLabel2],
				s.[IdentifierLabel3],
				s.[IdentifierVisibility],
				s.[VatRateVisibility],
				s.[DefaultVatRate],
				-- Inventory
				s.[ReorderLevelVisibility],
				s.[EconomicOrderQuantityVisibility],
				s.[UnitCardinality],
				s.[DefaultUnitId],
				s.[UnitMassVisibility],
				s.[DefaultUnitMassUnitId],
				-- Financial
				s.[MonetaryValueVisibility],
				s.[ParticipantVisibility],
				s.[ParticipantDefinitionId],

				s.[MainMenuIcon],
				s.[MainMenuSection],
				s.[MainMenuSortKey])		
		OUTPUT s.[Index], inserted.[Id]
	) AS x;
	
	-- Signal clients to refresh their cache
	IF EXISTS (SELECT * FROM @IndexedIds I JOIN [dbo].[ResourceDefinitions] D ON I.[Id] = D.[Id] WHERE D.[State] <> N'Hidden')
		UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;