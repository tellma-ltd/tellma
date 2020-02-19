CREATE PROCEDURE [dal].[ResourceDefinitions__Save]
	@Entities [ResourceDefinitionList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[ResourceDefinitions] AS t
	USING (
		SELECT 
			[Index],
			[Id],
			[TitleSingular],
			[TitleSingular2],
			[TitleSingular3],
			[TitlePlural],
			[TitlePlural2],
			[TitlePlural3],
			[ParentAccountTypeId],
			-- If null, no restriction. Otherwise, it restricts the types to those stemming from one of the nodes in the parent list
			--[CodeRegEx], -- Null means manually defined
			--[NameRegEx], -- Null means manually defined
			-- Resource properties
			-- [ResourceClassificationVisibility],
			[IdentifierLabel],
			[IdentifierLabel2],
			[IdentifierLabel3],		
			[IdentifierVisibility],

			[CurrencyVisibility],
			-- [CustomsReferenceVisibility],
			-- [PreferredSupplierVisibility],
			[DescriptionVisibility],
			[ReorderLevelVisibility],
			[EconomicOrderQuantityVisibility],

			[AvailableSinceLabel],
			[AvailableSinceLabel2],
			[AvailableSinceLabel3],
			[AvailableSinceVisibility],

			[AvailableTillLabel],
			[AvailableTillLabel2],
			[AvailableTillLabel3],
			[AvailableTillVisibility],

			[Decimal1Label],				
			[Decimal1Label2],
			[Decimal1Label3],
			[Decimal1Visibility],

			[Decimal2Label]	,
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

			[Lookup3Visibility],
			[Lookup3DefinitionId],
			[Lookup3Label],
			[Lookup3Label2],
			[Lookup3Label3],

			[Lookup4Visibility],
			[Lookup4DefinitionId],
			[Lookup4Label],
			[Lookup4Label2],
			[Lookup4Label3],

			[DueDateLabel],
			[DueDateLabel2],
			[DueDateLabel3],
			[DueDateVisibility],
			-- more properties from Resource Instances to come..
			[Text1Label],				
			[Text1Label2],
			[Text1Label3],
			[Text1Visibility],

			[Text2Label]	,
			[Text2Label2],
			[Text2Label3],
			[Text2Visibility],
			[MainMenuIcon],
			[MainMenuSection],			-- IF Null, it does not show on the main menu
			[MainMenuSortKey]
		FROM @Entities 
	) AS s ON (t.Id = s.[Id])
	WHEN MATCHED 
	THEN
		UPDATE SET 
			t.[TitleSingular] = s.[TitleSingular],
			t.[TitleSingular2] = s.[TitleSingular2],
			t.[TitleSingular3] = s.[TitleSingular3],
			t.[TitlePlural] = s.[TitlePlural],
			t.[TitlePlural2] = s.[TitlePlural2],
			t.[TitlePlural3] = s.[TitlePlural3],
			t.[ParentAccountTypeId] = s.[ParentAccountTypeId],
			-- If null, no restriction. Otherwise, it restricts the types to those stemming from one of the nodes in the parent list
			--t.[CodeRegEx] = s.[CodeRegEx], -- Null means manually defined
			--t.[NameRegEx] = s.[NameRegEx], -- Null means manually defined
			-- Resource properties
			t.[IdentifierLabel] = s.[IdentifierLabel],
			t.[IdentifierLabel2] = s.[IdentifierLabel2],
			t.[IdentifierLabel3] = s.[IdentifierLabel3],		
			t.[IdentifierVisibility] = s.[IdentifierVisibility],
			t.[CurrencyVisibility] = s.[CurrencyVisibility],
			-- [CustomsReferenceVisibility] = s.[CustomsReferenceVisibility],
			-- [PreferredSupplierVisibility] = s.[PreferredSupplierVisibility],
			t.[DescriptionVisibility] = s.[DescriptionVisibility],
			t.[ReorderLevelVisibility]=s.[ReorderLevelVisibility],
			t.[EconomicOrderQuantityVisibility]=s.[EconomicOrderQuantityVisibility],
			t.[AvailableSinceLabel] = s.[AvailableSinceLabel],
			t.[AvailableSinceLabel2] = s.[AvailableSinceLabel2],
			t.[AvailableSinceLabel3] = s.[AvailableSinceLabel3],
			t.[AvailableSinceVisibility] = s.[AvailableSinceVisibility],
			t.[AvailableTillLabel] = s.[AvailableTillLabel],
			t.[AvailableTillLabel2] = s.[AvailableTillLabel2],
			t.[AvailableTillLabel3] = s.[AvailableTillLabel3],
			t.[AvailableTillVisibility] = s.[AvailableTillVisibility],
			t.[Decimal1Label] = s.[Decimal1Label],						
			t.[Decimal1Label2] = s.[Decimal1Label2],
			t.[Decimal1Label3] = s.[Decimal1Label3],
			t.[Decimal1Visibility] = s.[Decimal1Visibility],

			t.[Decimal2Label] = s.[Decimal2Label],	
			t.[Decimal2Label2] = s.[Decimal2Label2],
			t.[Decimal2Label3] = s.[Decimal2Label3],
			t.[Decimal2Visibility] = s.[Decimal2Visibility],

			t.[Int1Label] = s.[Int1Label],		
			t.[Int1Label2] = s.[Int1Label2],
			t.[Int1Label3] = s.[Int1Label3],		
			t.[Int1Visibility] = s.[Int1Visibility],	

			t.[Int2Label] = s.[Int2Label],		
			t.[Int2Label2] = s.[Int2Label2],
			t.[Int2Label3] = s.[Int2Label3],	
			t.[Int2Visibility] = s.[Int2Visibility],

			t.[Lookup1Label] = s.[Lookup1Label],
			t.[Lookup1Label2] = s.[Lookup1Label2],
			t.[Lookup1Label3] = s.[Lookup1Label3],
			t.[Lookup1Visibility] = s.[Lookup1Visibility],
			t.[Lookup1DefinitionId] = s.[Lookup1DefinitionId],

			t.[Lookup2Label] = s.[Lookup2Label],
			t.[Lookup2Label2] = s.[Lookup2Label2],
			t.[Lookup2Label3] = s.[Lookup2Label3],
			t.[Lookup2Visibility] = s.[Lookup2Visibility],
			t.[Lookup2DefinitionId] = s.[Lookup2DefinitionId],

			t.[Lookup3Visibility] = s.[Lookup3Visibility],
			t.[Lookup3DefinitionId] = s.[Lookup3DefinitionId],
			t.[Lookup3Label] = s.[Lookup3Label],
			t.[Lookup3Label2] = s.[Lookup3Label2],
			t.[Lookup3Label3] = s.[Lookup3Label3],

			t.[Lookup4Visibility] = s.[Lookup4Visibility],
			t.[Lookup4DefinitionId] = s.[Lookup4DefinitionId],
			t.[Lookup4Label] = s.[Lookup4Label],
			t.[Lookup4Label2] = s.[Lookup4Label2],
			t.[Lookup4Label3] = s.[Lookup4Label3],

			t.[DueDateLabel] = s.[DueDateLabel],
			t.[DueDateLabel2] = s.[DueDateLabel2],
			t.[DueDateLabel3] = s.[DueDateLabel3],
			t.[DueDateVisibility] = s.[DueDateVisibility],
			-- more properties from Resource Instances to come..
			t.[Text1Label] = s.[Text1Label],						
			t.[Text1Label2] = s.[Text1Label2],
			t.[Text1Label3] = s.[Text1Label3],
			t.[Text1Visibility] = s.[Text1Visibility],

			t.[Text2Label] = s.[Text2Label],	
			t.[Text2Label2] = s.[Text2Label2],
			t.[Text2Label3] = s.[Text2Label3],
			t.[Text2Visibility] = s.[Text2Visibility],

			t.[MainMenuIcon] = s.[MainMenuIcon],
			t.[MainMenuSection] = s.[MainMenuSection],			-- IF Null, it does not show on the main menu
			t.[MainMenuSortKey] = s.[MainMenuSortKey],
			t.[SavedById]					= @UserId
	WHEN NOT MATCHED THEN
		INSERT (
			[Id],
			[TitleSingular],
			[TitleSingular2],
			[TitleSingular3],
			[TitlePlural],
			[TitlePlural2],
			[TitlePlural3],
			[ParentAccountTypeId],
			-- If null, no restriction. Otherwise, it restricts the types to those stemming from one of the nodes in the parent list
			--[CodeRegEx], -- Null means manually defined
			--[NameRegEx], -- Null means manually defined
			-- Resource properties
			[IdentifierLabel],
			[IdentifierLabel2],
			[IdentifierLabel3],		
			[IdentifierVisibility],
			[CurrencyVisibility],
			-- [CustomsReferenceVisibility],
			-- [PreferredSupplierVisibility],
			[DescriptionVisibility],
			[ReorderLevelVisibility],
			[EconomicOrderQuantityVisibility],
			[AvailableSinceLabel],
			[AvailableSinceLabel2],
			[AvailableSinceLabel3],
			[AvailableSinceVisibility],
			[AvailableTillLabel],
			[AvailableTillLabel2],
			[AvailableTillLabel3],
			[AvailableTillVisibility],
			[Decimal1Label],				
			[Decimal1Label2],
			[Decimal1Label3],
			[Decimal1Visibility],

			[Decimal2Label]	,
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

			[Lookup3Visibility],
			[Lookup3DefinitionId],
			[Lookup3Label],
			[Lookup3Label2],
			[Lookup3Label3],

			[Lookup4Visibility],
			[Lookup4DefinitionId],
			[Lookup4Label],
			[Lookup4Label2],
			[Lookup4Label3],

			[DueDateLabel],
			[DueDateLabel2],
			[DueDateLabel3],
			[DueDateVisibility],
			-- more properties from Resource Instances to come..
			[Text1Label],				
			[Text1Label2],
			[Text1Label3],
			[Text1Visibility],

			[Text2Label]	,
			[Text2Label2],
			[Text2Label3],
			[Text2Visibility],

			[MainMenuIcon],
			[MainMenuSection],			-- IF Null, it does not show on the main menu
			[MainMenuSortKey]
			
			)
		VALUES (
			s.[Id],
			s.[TitleSingular],
			s.[TitleSingular2],
			s.[TitleSingular3],
			s.[TitlePlural],
			s.[TitlePlural2],
			s.[TitlePlural3],
			s.[ParentAccountTypeId],
			-- If null, no restriction. Otherwise, it restricts the types to those stemming from one of the nodes in the parent list
			--s.[CodeRegEx], -- Null means manually defined
			--s.[NameRegEx], -- Null means manually defined
			-- Resource properties
			s.[IdentifierLabel],
			s.[IdentifierLabel2],
			s.[IdentifierLabel3],		
			s.[IdentifierVisibility],
			s.[CurrencyVisibility],
			-- [CustomsReferenceVisibility],
			-- [PreferredSupplierVisibility],
			s.[DescriptionVisibility],
			s.[ReorderLevelVisibility],
			s.[EconomicOrderQuantityVisibility],
			s.[AvailableSinceLabel],
			s.[AvailableSinceLabel2],
			s.[AvailableSinceLabel3],
			s.[AvailableSinceVisibility],
			s.[AvailableTillLabel],
			s.[AvailableTillLabel2],
			s.[AvailableTillLabel3],
			s.[AvailableTillVisibility],

			s.[Decimal1Label],				
			s.[Decimal1Label2],
			s.[Decimal1Label3],
			s.[Decimal1Visibility],

			s.[Decimal2Label]	,
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

			s.[Lookup3Visibility],
			s.[Lookup3DefinitionId],
			s.[Lookup3Label],
			s.[Lookup3Label2],
			s.[Lookup3Label3],

			s.[Lookup4Visibility],
			s.[Lookup4DefinitionId],
			s.[Lookup4Label],
			s.[Lookup4Label2],
			s.[Lookup4Label3],

			s.[DueDateLabel],
			s.[DueDateLabel2],
			s.[DueDateLabel3],
			s.[DueDateVisibility],
			-- more properties from Resource Instances to come..
			s.[Text1Label],				
			s.[Text1Label2],
			s.[Text1Label3],
			s.[Text1Visibility],

			s.[Text2Label]	,
			s.[Text2Label2],
			s.[Text2Label3],
			s.[Text2Visibility],

			s.[MainMenuIcon],
			s.[MainMenuSection],			-- IF Null, it does not show on the main menu
			s.[MainMenuSortKey]			
			);
