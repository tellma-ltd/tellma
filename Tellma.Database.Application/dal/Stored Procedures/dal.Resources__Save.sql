CREATE PROCEDURE [dal].[Resources__Save]
	@DefinitionId INT,
	@Entities [dbo].[ResourceList] READONLY,
	@ResourceUnits [dbo].[ResourceUnitList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList], @DeletedImageIds [dbo].[StringList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	
	-- Entities whose ImageIds will be updated: capture their old ImageIds first (if any) so C# can delete them from blob storage
	INSERT INTO @DeletedImageIds ([Id])
	SELECT [ImageId] FROM [dbo].[Resources] E
	WHERE E.[ImageId] IS NOT NULL 
		AND E.[Id] IN (SELECT [Id] FROM @Entities WHERE [ImageId] IS NULL OR [ImageId] <> N'(Unchanged)');

	-- Save entities
	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Resources] AS t
		USING (
			SELECT 
				[Index], [Id],
				@DefinitionId AS [DefinitionId],
				[Name], 
				[Name2], 
				[Name3],
				[Code],
				[CurrencyId],
				[CenterId],
				[Description],
				[Description2],
				[Description3],
				geography::STGeomFromWKB([LocationWkb], 4326) AS [Location], -- 4326 = World Geodetic System, used by Google Maps
				[LocationJson],
				[FromDate],
				[ToDate],
				[Decimal1],
				[Decimal2],
				[Int1],
				[Int2],
				[Lookup1Id],
				[Lookup2Id],
				[Lookup3Id],
				[Lookup4Id],
				[Text1],					
				[Text2],
-- Specific to resources
				[Identifier],
				[VatRate],
				[ReorderLevel],
				[EconomicOrderQuantity],	
				[UnitId],
				[UnitMass],
				[UnitMassUnitId],
				[MonetaryValue],
				[ParticipantId],
				[Resource1Id],
				[ImageId]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[DefinitionId]			= s.[DefinitionId],
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				
				t.[Code]					= s.[Code],
				t.[CurrencyId]				= s.[CurrencyId],
				t.[CenterId]				= s.[CenterId],
				t.[Description]				= s.[Description],
				t.[Description2]			= s.[Description2],
				t.[Description3]			= s.[Description3],
				t.[Location]				= s.[Location],
				t.[LocationJson]			= s.[LocationJson],
				t.[FromDate]				= s.[FromDate],
				t.[ToDate]					= s.[ToDate],
				t.[Decimal1]				= s.[Decimal1],
				t.[Decimal2]				= s.[Decimal2],
				t.[Int1]					= s.[Int1],
				t.[Int2]					= s.[Int2],
				t.[Lookup1Id]				= s.[Lookup1Id],
				t.[Lookup2Id]				= s.[Lookup2Id],
				t.[Lookup3Id]				= s.[Lookup3Id],
				t.[Lookup4Id]				= s.[Lookup4Id],
				t.[Text1]					= s.[Text1],	
				t.[Text2]					= s.[Text2],

				t.[Identifier]				= s.[Identifier],
				t.[VatRate]					= s.[VatRate],
				t.[ReorderLevel]			= s.[ReorderLevel],
				t.[EconomicOrderQuantity]	= s.[EconomicOrderQuantity],
				t.[UnitId]					= s.[UnitId],
				t.[UnitMass]				= s.[UnitMass],
				t.[UnitMassUnitId]			= s.[UnitMassUnitId],
				t.[MonetaryValue]			= s.[MonetaryValue],
				t.[ParticipantId]			= s.[ParticipantId],
				t.[Resource1Id]				= s.[Resource1Id],
				t.[ImageId]					= IIF(s.[ImageId] = N'(Unchanged)', t.[ImageId], s.[ImageId]),
				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[DefinitionId],
				[Name], 
				[Name2], 
				[Name3],
				[Code],
				[CurrencyId],
				[CenterId],
				[Description],
				[Description2],
				[Description3],
				[Location], -- 4326 = World Geodetic System, used by Google Maps
				[LocationJson],
				[FromDate],
				[ToDate],
				[Decimal1],
				[Decimal2],
				[Int1],
				[Int2],
				[Lookup1Id],
				[Lookup2Id],
				[Lookup3Id],
				[Lookup4Id],
				[Text1],					
				[Text2],
-- Specific to resources
				[Identifier],
				[VatRate],
				[ReorderLevel],
				[EconomicOrderQuantity],
				[UnitId],
				[UnitMass],
				[UnitMassUnitId],
				[MonetaryValue],
				[ParticipantId],
				[Resource1Id],
				[ImageId],
				[CreatedById], 
				[CreatedAt], 
				[ModifiedById], 
				[ModifiedAt]
				)
			VALUES (
				s.[DefinitionId],
				s.[Name], 
				s.[Name2], 
				s.[Name3],
				s.[Code],
				s.[CurrencyId],
				s.[CenterId],
				s.[Description],
				s.[Description2],
				s.[Description3],
				s.[Location], -- 4326 = World Geodetic System, used by Google Maps
				s.[LocationJson],
				s.[FromDate],
				s.[ToDate],
				s.[Decimal1],
				s.[Decimal2],
				s.[Int1],
				s.[Int2],
				s.[Lookup1Id],
				s.[Lookup2Id],
				s.[Lookup3Id],
				s.[Lookup4Id],
				s.[Text1],					
				s.[Text2],
-- Specific to resources
				s.[Identifier],
				s.[VatRate],
				s.[ReorderLevel],
				s.[EconomicOrderQuantity],
				s.[UnitId],
				s.[UnitMass],
				s.[UnitMassUnitId],
				s.[MonetaryValue],
				s.[ParticipantId],
				s.[Resource1Id],
				IIF(s.[ImageId] = N'(Unchanged)', NULL, s.[ImageId]),
				@UserId,
				@Now,
				@UserId,
				@Now
				)
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	WITH BU AS (
		SELECT * FROM [dbo].[ResourceUnits] RU
		WHERE RU.ResourceId IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO BU AS t
	USING (
		SELECT
			RU.[Id],
			I.[Id] AS [ResourceId],
			RU.[UnitId]
		FROM @ResourceUnits RU
		JOIN @IndexedIds I ON RU.[HeaderIndex] = I.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED AND (t.[UnitId] <> s.[UnitId])
	THEN
		UPDATE SET
			t.[UnitId]					= s.[UnitId],
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT (
			[ResourceId],
			[UnitId],
			[CreatedById], 
			[CreatedAt], 
			[ModifiedById], 
			[ModifiedAt]
		) VALUES (
			s.[ResourceId],
			s.[UnitId],
			@UserId,
			@Now,
			@UserId,
			@Now
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;		

	-- Return overwritten Image Ids, so C# can delete them from Blob Storage
	SELECT [Id] FROM @DeletedImageIds;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END;
