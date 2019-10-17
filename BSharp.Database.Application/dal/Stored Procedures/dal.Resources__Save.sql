CREATE PROCEDURE [dal].[Resources__Save]
	@DefinitionId NVARCHAR (255),
	@Entities [dbo].[ResourceList] READONLY,
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
				[Name], 
				[Name2], 
				[Name3], 
				[Code], 
				[ResourceTypeId], 
				[ResourceClassificationId], 
				--[UnitArea], 
				[AreaUnitId],
				[CountUnitId],
				--[UnitLength], 
				[LengthUnitId], 
				--[UnitMass], 
				[MassUnitId],
				--[UnitMonetaryValue], 
				[MonetaryValueCurrencyId],
				--[UnitTime], 
				[TimeUnitId], 
				--[UnitVolume], 
				[VolumeUnitId],
				[Description],
				[AttachmentsFolderURL],		-- Comment

				[CustomsReference], -- how it is referred to by Customs
				--[PreferredSupplierId]			INT,			-- FK, Table Agents, specially for purchasing

				[AvailableSince],			-- Comment
				[AvailableTill],			-- Comment
				[GloballyUniqueReference],	-- Comment
	
				[AssetAccountId],			-- Comment
				[LiabilityAccountId],		-- Comment
				[EquityAccountId],			-- Comment
				[RevenueAccountId],			-- Comment
				[ExpensesAccountId],		-- Comment

				[Agent1Id],					-- Comment
				[Agent2Id],					-- Comment
				[Date1]	,					-- Comment
				[Date2],					-- Comment

				[Lookup1Id],
				[Lookup2Id],
				[Lookup3Id],
				[Lookup4Id],
				[Lookup5Id],				-- Comment
				[Text1],					-- Comment
				[Text2]						-- Comment
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[Code]					= s.[Code],
				t.[ResourceTypeId]			= s.[ResourceTypeId],     
				t.[ResourceClassificationId]= s.[ResourceClassificationId],     
				--t.[UnitMonetaryValue]		= s.[UnitMonetaryValue],
				t.[MonetaryValueCurrencyId]				= s.[MonetaryValueCurrencyId],
				--t.[UnitMass]				= s.[UnitMass],
				t.[MassUnitId]				= s.[MassUnitId],
				--t.[UnitVolume]				= s.[UnitVolume],
				t.[VolumeUnitId]			= s.[VolumeUnitId],
				t.[LengthUnitId]			= s.[LengthUnitId],
				t.[AreaUnitId]				= s.[AreaUnitId],
				--t.[UnitTime]				= s.[UnitTime],
				t.[TimeUnitId]				= s.[TimeUnitId],
				t.[CountUnitId]				= s.[CountUnitId],
				t.[Description]					= s.[Description],      
				t.[CustomsReference]		= s.[CustomsReference],
				--t.[PreferredSupplierId]		= s.[PreferredSupplierId],
				t.[Lookup1Id]				= s.[Lookup1Id],
				t.[Lookup2Id]				= s.[Lookup2Id],
				t.[Lookup3Id]				= s.[Lookup3Id],
				t.[Lookup4Id]				= s.[Lookup4Id],
				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([ResourceDefinitionId], [Name], [Name2], [Name3], [Code], [ResourceTypeId],  [ResourceClassificationId], 
				--[UnitMonetaryValue], 
				[MonetaryValueCurrencyId], 
				--[UnitMass], 
				[MassUnitId], 
				--[UnitVolume], 
				[VolumeUnitId],
				--[UnitArea],
				[AreaUnitId],
				--[UnitLength],
				[LengthUnitId],
				--[UnitTime],
				[TimeUnitId],
				[CountUnitId],
				[Description], 
				[CustomsReference] ,
				--[PreferredSupplierId],
				[Lookup1Id], [Lookup2Id], [Lookup3Id], [Lookup4Id],
				[AvailableSince], [Text1]
				)
			VALUES (@DefinitionId, s.[Name], s.[Name2], s.[Name3], s.[Code], s.[ResourceTypeId], s.[ResourceClassificationId],
				--s.[UnitMonetaryValue],
				s.[MonetaryValueCurrencyId],
				--s.[UnitMass],
				s.[MassUnitId],
				--s.[UnitVolume],
				s.[VolumeUnitId],
				--s.[UnitArea],
				s.[AreaUnitId],
				--s.[UnitLength],
				s.[LengthUnitId],
				--s.[UnitTime],
				s.[TimeUnitId],
				s.[CountUnitId],
				s.[Description],
				s.[CustomsReference],
				--s.[PreferredSupplierId],
				s.[Lookup1Id], s.[Lookup2Id], s.[Lookup3Id], s.[Lookup4Id],
				s.[AvailableSince], s.[Text1]
				)
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
