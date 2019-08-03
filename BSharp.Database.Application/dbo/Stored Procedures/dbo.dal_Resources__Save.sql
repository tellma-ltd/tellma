CREATE PROCEDURE [dbo].[dal_Resources__Save]
	@Resources [dbo].[ResourceList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[Resources] AS t
	USING (
		SELECT 	
			[Index], [Id], [ResourceType], [Name], [Name2], [Name3], [IsFungible], [IsBatch], 
			[ValueMeasure], [UnitId], [CurrencyId], [UnitPrice], [MassUnitId], [UnitMass], [VolumeUnitId], [UnitVolume],
			[AreaUnitId], [UnitArea], [LengthUnitId], [UnitLength], [TimeUnitId], [UnitTime], [CountUnitId],
			[Code], [SystemCode], [Memo], [CustomsReference] ,[UniversalProductCode], [PreferredSupplierId],
			[ResourceLookup1Id], [ResourceLookup2Id], [ResourceLookup3Id], [ResourceLookup4Id]
		FROM @Resources 
		WHERE [EntityState] IN (N'Inserted', N'Updated')
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED 
	THEN
		UPDATE SET 
			t.[ResourceType]			= s.[ResourceType],     
			t.[Name]					= s.[Name],
			t.[Name2]					= s.[Name2],
			t.[Name3]					= s.[Name3],
			t.[IsFungible]				= s.[IsFungible],
			t.[IsBatch]					= s.[IsBatch],
			t.[ValueMeasure]			= s.[ValueMeasure],
			t.[UnitId]					= s.[UnitId],
			t.[CurrencyId]				= s.[CurrencyId],
			t.[UnitPrice]				= s.[UnitPrice],
			t.[MassUnitId]				= s.[MassUnitId],
			t.[UnitMass]				= s.[UnitMass],
			t.[VolumeUnitId]			= s.[VolumeUnitId],
			t.[UnitVolume]				= s.[UnitVolume],
			t.[AreaUnitId]				= s.[AreaUnitId],
			t.[LengthUnitId]			= s.[LengthUnitId],
			t.[TimeUnitId]				= s.[TimeUnitId],
			t.[UnitTime]				= s.[UnitTime],
			t.[CountUnitId]				= s.[CountUnitId],
			t.[Code]					= s.[Code],
			t.[SystemCode]				= s.[SystemCode],
			t.[Memo]					= s.[Memo],      
			t.[CustomsReference]		= s.[CustomsReference],
			t.[UniversalProductCode]	= s.[UniversalProductCode],
			t.[PreferredSupplierId]		= s.[PreferredSupplierId],
			t.[ResourceLookup1Id]		= s.[ResourceLookup1Id],
			t.[ResourceLookup2Id]		= s.[ResourceLookup2Id],
			t.[ResourceLookup3Id]		= s.[ResourceLookup3Id],
			t.[ResourceLookup4Id]		= s.[ResourceLookup4Id],
			t.[ModifiedAt]				= @Now,
			t.[ModifiedById]			= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([ResourceType], [Name], [Name2], [Name3], [IsFungible], [IsBatch],
			[ValueMeasure], [UnitId], [CurrencyId], [MassUnitId], [UnitMass], [VolumeUnitId], [UnitVolume],
			[AreaUnitId], [UnitArea], [LengthUnitId], [UnitLength], [TimeUnitId], [UnitTime], [CountUnitId],
			[Code], [SystemCode], [Memo], [CustomsReference] ,[UniversalProductCode], [PreferredSupplierId],
			[ResourceLookup1Id], [ResourceLookup2Id], [ResourceLookup3Id], [ResourceLookup4Id])
		VALUES (s.[ResourceType], s.[Name], s.[Name2], s.[Name3], s.[IsFungible], s.[IsBatch],
			s.[ValueMeasure], s.[UnitId], s.[CurrencyId], s.[MassUnitId], s.[UnitMass], s.[VolumeUnitId], s.[UnitVolume],
			s.[AreaUnitId], s.[UnitArea], s.[LengthUnitId], s.[UnitLength], s.[TimeUnitId], s.[UnitTime], s.[CountUnitId],
			s.[Code], s.[SystemCode], s.[Memo], s.[CustomsReference] ,s.[UniversalProductCode], s.[PreferredSupplierId],
			s.[ResourceLookup1Id], s.[ResourceLookup2Id], s.[ResourceLookup3Id], s.[ResourceLookup4Id])
		OUTPUT s.[Index], inserted.[Id];