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
				[OperatingSegmentId],
				[ResourceTypeId],
				@DefinitionId AS [DefinitionId],
				[ResourceClassificationId], 
				[Name], 
				[Name2], 
				[Name3],
				[DescriptorId],
				[Code], 

				[CurrencyId],
				[MonetaryValue],
				[CountUnitId],
				[Count],
				[MassUnitId],
				[Mass],
				[VolumeUnitId],
				[Volume],
				[TimeUnitId], 
				[Time],
				[Description],
				[Description2],
				[Description3],
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

				[Lookup1Id],
				[Lookup2Id]
				--[Lookup3Id],
				--[Lookup4Id],
				--[Lookup5Id],				
				--[Text1],					
				--[Text2]						
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[OperatingSegmentId]		= s.[OperatingSegmentId],
				t.[ResourceTypeId]			= s.[ResourceTypeId],  
				t.[DefinitionId]			= s.[DefinitionId],
				t.[ResourceClassificationId]= s.[ResourceClassificationId], 
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[DescriptorId]			= s.[DescriptorId],
				t.[Code]					= s.[Code],

				t.[CurrencyId]				= s.[CurrencyId],
				t.[MonetaryValue]			= s.[MonetaryValue],
				t.[CountUnitId]				= s.[CountUnitId],
				t.[Count]					= s.[Count],
				t.[MassUnitId]				= s.[MassUnitId],
				t.[Mass]					= s.[Mass],
				t.[VolumeUnitId]			= s.[VolumeUnitId],
				t.[Volume]					= s.[Volume],
				t.[TimeUnitId]				= s.[TimeUnitId],
				t.[Time]					= s.[Time],

				t.[Description]				= s.[Description],
				t.[Description2]			= s.[Description2],
				t.[Description3]			= s.[Description3],
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
				t.[Lookup1Id]				= s.[Lookup1Id],
				t.[Lookup2Id]				= s.[Lookup2Id],
				--t.[Lookup3Id]				= s.[Lookup3Id],
				--t.[Lookup4Id]				= s.[Lookup4Id],
				--t.[Lookup5Id]				= s.[Lookup5Id],
				--t.[Text1]					= s.[Text1],					
				--t.[Text2]					= s.[Text2],	

				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([OperatingSegmentId], [ResourceTypeId], [DefinitionId], [ResourceClassificationId], [Name], [Name2], [Name3], [DescriptorId], [Code],
				[CurrencyId],
				[MonetaryValue],
				[CountUnitId],
				[Count],
				[MassUnitId],
				[Mass],
				[VolumeUnitId],
				[Volume],
				[TimeUnitId], 
				[Time],
				[Description],
				[Description2],
				[Description3],
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

				[Lookup1Id],
				[Lookup2Id]
				--[Lookup3Id],
				--[Lookup4Id],
				--[Lookup5Id],				
				--[Text1],					
				--[Text2]			
				)
			VALUES (s.[OperatingSegmentId], s.[ResourceTypeId], s.[DefinitionId], s.[ResourceClassificationId], s.[Name], s.[Name2], s.[Name3], s.[DescriptorId], s.[Code],
				s.[CurrencyId],
				s.[MonetaryValue],
				s.[CountUnitId],
				s.[Count],
				s.[MassUnitId],
				s.[Mass],
				s.[VolumeUnitId],
				s.[Volume],
				s.[TimeUnitId], 
				s.[Time],
				s.[Description],
				s.[Description2],
				s.[Description3],
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

				s.[Lookup1Id],
				s.[Lookup2Id]
				--s.[Lookup3Id],
				--s.[Lookup4Id],
				--s.[Lookup5Id],				
				--s.[Text1],					
				--s.[Text2]			
				)
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
