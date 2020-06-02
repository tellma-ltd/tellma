CREATE PROCEDURE [dal].[ReportDefinitions__Save]
	@Entities [ReportDefinitionList] READONLY,
	@Parameters [ReportParameterDefinitionList] READONLY,
	@Select [ReportSelectDefinitionList] READONLY,
	@Rows [ReportDimensionDefinitionList] READONLY,
	@Columns [ReportDimensionDefinitionList] READONLY,
	@Measures [ReportMeasureDefinitionList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Report Definitions
	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[ReportDefinitions] AS t
		USING (
			SELECT 
				[Index], [Id], [Title], [Title2], [Title3], [Description], [Description2], [Description3],
				[Type], [Chart], [DefaultsToChart], [Collection], [DefinitionId], [Filter], [OrderBy], [Top],
				[ShowColumnsTotal], [ShowRowsTotal], [ShowInMainMenu], [MainMenuSection], [MainMenuIcon], [MainMenuSortKey]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Title]				= s.[Title],
				t.[Title2]				= s.[Title2],
				t.[Title3]				= s.[Title3],
				t.[Description]			= s.[Description],
				t.[Description2]		= s.[Description2],
				t.[Description3]		= s.[Description3],
				t.[Type]				= s.[Type],
				t.[Chart]				= s.[Chart],
				t.[DefaultsToChart]		= s.[DefaultsToChart],
				t.[Collection]			= s.[Collection],
				t.[DefinitionId]		= s.[DefinitionId],
				t.[Filter]				= s.[Filter],
				t.[OrderBy]				= s.[OrderBy],
				t.[Top]					= s.[Top],
				t.[ShowColumnsTotal]	= s.[ShowColumnsTotal],
				t.[ShowRowsTotal]		= s.[ShowRowsTotal],
				t.[ShowInMainMenu]		= s.[ShowInMainMenu],
				t.[MainMenuSection]		= s.[MainMenuSection],
				t.[MainMenuIcon]		= s.[MainMenuIcon],
				t.[MainMenuSortKey]		= s.[MainMenuSortKey],			
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[Title], [Title2], [Title3], [Description], [Description2], [Description3],
				[Type], [Chart], [DefaultsToChart], [Collection], [DefinitionId], [Filter], [OrderBy], [Top],
				[ShowColumnsTotal], [ShowRowsTotal], [ShowInMainMenu], [MainMenuSection], [MainMenuIcon], [MainMenuSortKey]
			)
			VALUES (
				s.[Title], s.[Title2], s.[Title3], s.[Description], s.[Description2], s.[Description3],
				s.[Type], s.[Chart], s.[DefaultsToChart], s.[Collection], s.[DefinitionId], s.[Filter], s.[OrderBy], s.[Top],
				s.[ShowColumnsTotal], s.[ShowRowsTotal], s.[ShowInMainMenu], s.[MainMenuSection], s.[MainMenuIcon], s.[MainMenuSortKey]
			)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	-- Parameters Definitions
	WITH BP AS (
		SELECT * FROM [dbo].[ReportParameterDefinitions]
		WHERE [ReportDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BP AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [ReportDefinitionId], L.[Key], L.[Label], L.[Label2], L.[Label3], L.[Visibility], L.[Value]
		FROM @Parameters L
		JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[Key]					= s.[Key],
			t.[Label]				= s.[Label],
			t.[Label2]				= s.[Label2],
			t.[Label3]				= s.[Label3],
			t.[Visibility]			= s.[Visibility],
			t.[Value]				= s.[Value]
	WHEN NOT MATCHED THEN
		INSERT (
			[Index], [ReportDefinitionId], [Key], [Label], [Label2], [Label3], [Visibility], [Value]
		)
		VALUES (
			s.[Index], s.[ReportDefinitionId], s.[Key], s.[Label], s.[Label2], s.[Label3], s.[Visibility], s.[Value]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;
	
	-- Select Definitions
	WITH BS AS (
		SELECT * FROM [dbo].[ReportSelectDefinitions]
		WHERE [ReportDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BS AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [ReportDefinitionId], L.[Path], L.[Label], L.[Label2], L.[Label3]
		FROM @Select L JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[Path]				= s.[Path],
			t.[Label]				= s.[Label],
			t.[Label2]				= s.[Label2],
			t.[Label3]				= s.[Label3]
	WHEN NOT MATCHED THEN
		INSERT (
			[Index], [ReportDefinitionId], [Path], [Label], [Label2], [Label3]
		)
		VALUES (
			s.[Index], s.[ReportDefinitionId], s.[Path], s.[Label], s.[Label2], s.[Label3]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Rows Definitions
	WITH BR AS (
		SELECT * FROM [dbo].[ReportDimensionDefinitions]
		WHERE [Discriminator] = N'Row' AND [ReportDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BR AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [ReportDefinitionId], L.[Path], L.[Modifier], L.[Label], L.[Label2], L.[Label3], L.[OrderDirection], L.[AutoExpand]
		FROM @Rows L JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[Path]				= s.[Path],
			t.[Modifier]			= s.[Modifier],
			t.[Label]				= s.[Label],
			t.[Label2]				= s.[Label2],
			t.[Label3]				= s.[Label3],
			t.[OrderDirection]		= s.[OrderDirection],
			t.[AutoExpand]			= s.[AutoExpand]
	WHEN NOT MATCHED THEN
		INSERT (
			[Index], [Discriminator], [ReportDefinitionId], [Path], [Modifier], [Label], [Label2], [Label3], [OrderDirection], [AutoExpand]
		)
		VALUES (
			s.[Index], N'Row', s.[ReportDefinitionId], s.[Path], s.[Modifier], s.[Label], s.[Label2], s.[Label3], s.[OrderDirection], s.[AutoExpand]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Columns Definitions
	WITH BC AS (
		SELECT * FROM [dbo].[ReportDimensionDefinitions]
		WHERE [Discriminator] = N'Column' AND [ReportDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BC AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [ReportDefinitionId], L.[Path], L.[Modifier], L.[Label], L.[Label2], L.[Label3], L.[OrderDirection], L.[AutoExpand]
		FROM @Columns L JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[Path]				= s.[Path],
			t.[Modifier]			= s.[Modifier],
			t.[Label]				= s.[Label],
			t.[Label2]				= s.[Label2],
			t.[Label3]				= s.[Label3],
			t.[OrderDirection]		= s.[OrderDirection],
			t.[AutoExpand]			= s.[AutoExpand]
	WHEN NOT MATCHED THEN
		INSERT (
			[Index], [Discriminator], [ReportDefinitionId], [Path], [Modifier], [Label], [Label2], [Label3], [OrderDirection], [AutoExpand]
		)
		VALUES (
			s.[Index], N'Column', s.[ReportDefinitionId], s.[Path], s.[Modifier], s.[Label], s.[Label2], s.[Label3], s.[OrderDirection], s.[AutoExpand]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;
				
	-- Measure Definitions
	WITH BM AS (
		SELECT * FROM [dbo].[ReportMeasureDefinitions]
		WHERE [ReportDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BM AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [ReportDefinitionId], L.[Path], L.[Label], L.[Label2], L.[Label3], L.[OrderDirection], L.[Aggregation]
		FROM @Measures L JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[Path]				= s.[Path],
			t.[Label]				= s.[Label],
			t.[Label2]				= s.[Label2],
			t.[Label3]				= s.[Label3],
			t.[OrderDirection]		= s.[OrderDirection],
			t.[Aggregation]			= s.[Aggregation]
	WHEN NOT MATCHED THEN
		INSERT (
			[Index], [ReportDefinitionId], [Path], [Label], [Label2], [Label3], [OrderDirection], [Aggregation]
		)
		VALUES (
			s.[Index], s.[ReportDefinitionId], s.[Path], s.[Label], s.[Label2], s.[Label3], s.[OrderDirection], s.[Aggregation]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Signal clients to refresh their cache
	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();