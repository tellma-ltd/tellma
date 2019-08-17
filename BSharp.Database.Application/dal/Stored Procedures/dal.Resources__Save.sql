CREATE PROCEDURE [dal].[Resources__Save]
	@Resources [dbo].[ResourceList] READONLY,
	@Instances [dbo].[ResourceInstanceList] READONLY,
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
				[Index], [Id], [ResourceType], [Name], [Name2], [Name3], Uniqueness, [IsBatch], 
				[ValueMeasure], [CurrencyId], [UnitMoney], [MassUnitId], [UnitMass], [VolumeUnitId], [UnitVolume],
				[AreaUnitId], [UnitArea], [LengthUnitId], [UnitLength], [TimeUnitId], [UnitTime], [CountUnitId],
				[Code], [SystemCode], [Memo], [CustomsReference] ,[UniversalProductCode], [PreferredSupplierId],
				[ResourceLookup1Id], [ResourceLookup2Id], [ResourceLookup3Id], [ResourceLookup4Id]
			FROM @Resources 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[ResourceType]			= s.[ResourceType],     
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[Uniqueness]				= s.Uniqueness,
				t.[IsBatch]					= s.[IsBatch],
				t.[ValueMeasure]			= s.[ValueMeasure],
				t.[CurrencyId]				= s.[CurrencyId],
				t.[UnitMoney]				= s.[UnitMoney],
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
			INSERT ([ResourceType], [Name], [Name2], [Name3], [Uniqueness], [IsBatch],
				[ValueMeasure], [CurrencyId], [MassUnitId], [UnitMass], [VolumeUnitId], [UnitVolume],
				[AreaUnitId], [UnitArea], [LengthUnitId], [UnitLength], [TimeUnitId], [UnitTime], [CountUnitId],
				[Code], [SystemCode], [Memo], [CustomsReference] ,[UniversalProductCode], [PreferredSupplierId],
				[ResourceLookup1Id], [ResourceLookup2Id], [ResourceLookup3Id], [ResourceLookup4Id])
			VALUES (s.[ResourceType], s.[Name], s.[Name2], s.[Name3], s.Uniqueness, s.[IsBatch],
				s.[ValueMeasure], s.[CurrencyId], s.[MassUnitId], s.[UnitMass], s.[VolumeUnitId], s.[UnitVolume],
				s.[AreaUnitId], s.[UnitArea], s.[LengthUnitId], s.[UnitLength], s.[TimeUnitId], s.[UnitTime], s.[CountUnitId],
				s.[Code], s.[SystemCode], s.[Memo], s.[CustomsReference] ,s.[UniversalProductCode], s.[PreferredSupplierId],
				s.[ResourceLookup1Id], s.[ResourceLookup2Id], s.[ResourceLookup3Id], s.[ResourceLookup4Id])
			OUTPUT s.[Index], inserted.[Id]
	) AS x
	OPTION (RECOMPILE);

	WITH BE AS (
		SELECT * FROM dbo.[ResourceInstances]
		WHERE ResourceId IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO BE AS t
	USING (
		SELECT RI.[Id], II.[Id] As ResourceId, RI.[Code], RI.[ProductionDate],
				[MoneyAmount], [Mass], [Volume], [Area], [Length], [Time]
		FROM @Instances RI
		JOIN @IndexedIds II ON RI.[ResourceIndex] = II.[Index]
	) AS s
	ON (s.[Id] = t.[Id])
	WHEN MATCHED THEN
		UPDATE SET
			t.[Code]			= s.[Code],
			t.[ProductionDate]	= s.[ProductionDate],
			t.[MoneyAmount]			= s.[MoneyAmount],
			t.[Mass]			= s.[Mass],
			t.[Volume]			= s.[Volume],
			t.[Area]			= s.[Area],
			t.[Length]			= s.[Length],
			t.[Time]			= s.[Time]
	WHEN NOT MATCHED THEN
		INSERT ([ResourceId], [ProductionDate], [Code], [MoneyAmount], [Mass], [Volume], [Area], [Length], [Time])
		VALUES(s.[ResourceId], s.[ProductionDate], s.[Code], s.[MoneyAmount], s.[Mass], s.[Volume], s.[Area], s.[Length], s.[Time])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
