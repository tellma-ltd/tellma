CREATE PROCEDURE [dal].[Resources__Save]
	@DefinitionId INT,
	@Entities [dbo].[ResourceList] READONLY,
	@ResourceUnits dbo.ResourceUnitList READONLY,
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
		MERGE INTO [dbo].[Resources] AS t
		USING (
			SELECT 	
				[Index], [Id],
				@DefinitionId AS [DefinitionId],
				[AssetTypeId],
				[ExpenseTypeId],
				[RevenueTypeId],
				[Name], 
				[Name2], 
				[Name3],
				[Identifier],
				[Code], 
				[CurrencyId],
				[MonetaryValue],
				[Description],
				[Description2],
				[Description3],
				[ExpenseEntryTypeId],
				[CenterId],
				[ResidualMonetaryValue],
				[ResidualValue],
				[ReorderLevel],
				[EconomicOrderQuantity],
				--[AttachmentsFolderURL],		

				--[CustomsReference], -- how it is referred to by Customs
				--[PreferredSupplierId]			INT,			-- FK, Table Agents, specially for purchasing

				[AvailableSince],			
				[AvailableTill],			
				--[UniqueReference1],			
	
				--[AssetAccountId],			
				--[LiabilityAccountId],		
				--[EquityAccountId],			
				--[RevenueAccountId],			
				--[ExpensesAccountId],		

				--[Agent1Id],					
				--[Agent2Id],					
				--[Date1]	,					
				--[Date2],					
				[Decimal1],
				[Decimal2],
				[Int1],
				[Int2],
				[Lookup1Id],
				[Lookup2Id],
				[Lookup3Id],
				[Lookup4Id],
				--[Lookup5Id],				
				[Text1],					
				[Text2]						
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[DefinitionId]			= s.[DefinitionId],
				t.[AssetTypeId]				= s.[AssetTypeId],
				t.[ExpenseTypeId]			= s.[ExpenseTypeId],
				t.[RevenueTypeId]			= s.[RevenueTypeId],
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[Identifier]				= s.[Identifier],
				t.[Code]					= s.[Code],
				t.[CurrencyId]				= s.[CurrencyId],
				t.[MonetaryValue]			= s.[MonetaryValue],
				t.[Description]				= s.[Description],
				t.[Description2]			= s.[Description2],
				t.[Description3]			= s.[Description3],
				t.[ExpenseEntryTypeId]		= s.[ExpenseEntryTypeId],
				t.[CenterId]				= s.[CenterId],
				t.[ResidualMonetaryValue]	= s.[ResidualMonetaryValue],
				t.[ResidualValue]			= s.[ResidualValue],
				t.[ReorderLevel]			= s.[ReorderLevel],
				t.[EconomicOrderQuantity]	= s.[EconomicOrderQuantity],
				--t.[AttachmentsFolderURL]	= s.[AttachmentsFolderURL],
				--t.[CustomsReference]		= s.[CustomsReference],
				t.[AvailableSince]			= s.[AvailableSince],			
				t.[AvailableTill]			= s.[AvailableTill],

				--t.[UniqueReference1]		= s.[UniqueReference1],
				--t.[AssetAccountId]			= s.[AssetAccountId],			
				--t.[LiabilityAccountId]		= s.[LiabilityAccountId],		
				--t.[EquityAccountId]			= s.[EquityAccountId],			
				--t.[RevenueAccountId]		= s.[RevenueAccountId],			
				--t.[ExpensesAccountId]		= s.[ExpensesAccountId],	

				--t.[Agent1Id]				= s.[Agent1Id],					
				--t.[Agent2Id]				= s.[Agent2Id],					
				--t.[Date1]					= s.[Date1],					
				--t.[Date2]					= s.[Date2],
				t.[Decimal1]				= s.[Decimal1],
				t.[Decimal2]				= s.[Decimal2],
				t.[Int1]					= s.[Int1],
				t.[Int2]					= s.[Int2],
				t.[Lookup1Id]				= s.[Lookup1Id],
				t.[Lookup2Id]				= s.[Lookup2Id],
				t.[Lookup3Id]				= s.[Lookup3Id],
				t.[Lookup4Id]				= s.[Lookup4Id],
				--t.[Lookup5Id]				= s.[Lookup5Id],
				t.[Text1]					= s.[Text1],					
				t.[Text2]					= s.[Text2],	

				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[DefinitionId],
				[AssetTypeId], 
				[ExpenseTypeId],
				[RevenueTypeId],
				[Name], [Name2], [Name3], [Identifier], [Code], [CurrencyId],
				[MonetaryValue],
				[Description],
				[Description2],
				[Description3],
				[ExpenseEntryTypeId],
				[CenterId],
				[ResidualMonetaryValue],
				[ResidualValue],
				[ReorderLevel],
				[EconomicOrderQuantity],
				--[AttachmentsFolderURL],		
				--[CustomsReference], -- how it is referred to by Customs
				--[PreferredSupplierId]			INT,			-- FK, Table Agents, specially for purchasing
				[AvailableSince],			
				[AvailableTill],			
				--[UniqueReference1],			
	
				--[AssetAccountId],			
				--[LiabilityAccountId],		
				--[EquityAccountId],			
				--[RevenueAccountId],			
				--[ExpensesAccountId],		

				--[Agent1Id],					
				--[Agent2Id],					
				--[Date1]	,					
				--[Date2],					
				[Decimal1],
				[Decimal2],
				[Int1],
				[Int2],
				[Lookup1Id],
				[Lookup2Id],
				[Lookup3Id],
				[Lookup4Id],
				--[Lookup5Id],				
				[Text1],					
				[Text2]			
				)
			VALUES (
				s.[DefinitionId],
				s.[AssetTypeId],
				s.[ExpenseTypeId],
				s.[RevenueTypeId],				
				s.[Name], s.[Name2], s.[Name3], s.[Identifier], s.[Code], s.[CurrencyId],
				s.[MonetaryValue],
				s.[Description],
				s.[Description2],
				s.[Description3],
				s.[ExpenseEntryTypeId],
				s.[CenterId],
				s.[ResidualMonetaryValue],
				s.[ResidualValue],
				s.[ReorderLevel],
				s.[EconomicOrderQuantity],
				--s.[AttachmentsFolderURL],		
				--s.[CustomsReference], -- how it is referred to by Customs
				--s.[PreferredSupplierId]			INT,			-- FK, Table Agents, specially for purchasing

				s.[AvailableSince],			
				s.[AvailableTill],			
				--s.[UniqueReference1],			
	
				--s.[AssetAccountId],			
				--s.[LiabilityAccountId],		
				--s.[EquityAccountId],			
				--s.[RevenueAccountId],			
				--s.[ExpensesAccountId],		

				--s.[Agent1Id],					
				--s.[Agent2Id],					
				--s.[Date1]	,					
				--s.[Date2],					
				s.[Decimal1],
				s.[Decimal2],
				s.[Int1],
				s.[Int2],
				s.[Lookup1Id],
				s.[Lookup2Id],
				s.[Lookup3Id],
				s.[Lookup4Id],
				--s.[Lookup5Id],				
				s.[Text1],					
				s.[Text2]			
				)
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	WITH BU AS (
		SELECT * FROM dbo.ResourceUnits RU
		WHERE RU.ResourceId IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO BU AS t
	USING (
		SELECT
			RU.[Id],
			I.[Id] AS [ResourceId],
			RU.[UnitId],
			RU.[Multiplier]
		FROM @ResourceUnits RU
		JOIN @IndexedIds I ON RU.[HeaderIndex] = I.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED THEN
		UPDATE SET
			t.[ResourceId]				= s.[ResourceId],
			t.[UnitId]					= s.[UnitId],
			t.[Multiplier]				= s.[Multiplier],
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT (
			[ResourceId],
			[UnitId],
			[Multiplier]
		) VALUES (
			s.[ResourceId],
			s.[UnitId],
			s.[Multiplier]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
