CREATE PROCEDURE [dal].[LookupDefinitions__Save]
	@Entities [LookupDefinitionList] READONLY,
	@ReportDefinitions [LookupDefinitionReportDefinitionList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[LookupDefinitions] AS t
		USING (
			SELECT [Index], [Id], [Code], [TitleSingular], [TitleSingular2], [TitleSingular3],
				[TitlePlural], [TitlePlural2], [TitlePlural3], [MainMenuIcon],
				[MainMenuSection], [MainMenuSortKey]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[Code]				= s.[Code],
				t.[TitleSingular]		= s.[TitleSingular],
				t.[TitleSingular2]		= s.[TitleSingular2],
				t.[TitleSingular3]		= s.[TitleSingular3],
				t.[TitlePlural]			= s.[TitlePlural],
				t.[TitlePlural2]		= s.[TitlePlural2],
				t.[TitlePlural3]		= s.[TitlePlural3],
				t.[MainMenuIcon]		= s.[MainMenuIcon],
				t.[MainMenuSection]		= s.[MainMenuSection],
				t.[MainMenuSortKey]		= s.[MainMenuSortKey],
				t.[SavedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([Code],	[TitleSingular],	[TitleSingular2], [TitleSingular3],		[TitlePlural],	[TitlePlural2],		[TitlePlural3], [MainMenuIcon],		[MainMenuSection], [MainMenuSortKey], [SavedById])
			VALUES (s.[Code], s.[TitleSingular], s.[TitleSingular2], s.[TitleSingular3], s.[TitlePlural], s.[TitlePlural2], s.[TitlePlural3], s.[MainMenuIcon], s.[MainMenuSection], s.[MainMenuSortKey], @UserId)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	WITH CurrentDefinitionReportDefinitions AS (
		SELECT *
		FROM [dbo].[LookupDefinitionReportDefinitions]
		WHERE [LookupDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE CurrentDefinitionReportDefinitions AS t
	USING (
		SELECT
			RDRD.[Index],
			RDRD.[Id],
			II.[Id] AS [LookupDefinitionId],
			RDRD.[ReportDefinitionId],
			RDRD.[Name],
			RDRD.[Name2],
			RDRD.[Name3]
		FROM @Entities DD
		JOIN @IndexedIds II ON DD.[Index] = II.[Index]
		JOIN @ReportDefinitions RDRD ON DD.[Index] = RDRD.[HeaderIndex]
	) AS s
	ON s.Id = t.Id
	WHEN MATCHED THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[ReportDefinitionId]	= s.[ReportDefinitionId],
			t.[Name]				= s.[Name],
			t.[Name2]				= s.[Name2],
			t.[Name3]				= s.[Name3],
			t.[SavedById]			= @UserId
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (
			[Index], [LookupDefinitionId],	[ReportDefinitionId], [Name], [Name2], [Name3], [SavedById]
		) VALUES (
			[Index], s.[LookupDefinitionId], s.[ReportDefinitionId], s.[Name], s.[Name2], s.[Name3], @UserId
		);
	
	-- Signal clients to refresh their cache
	IF EXISTS (SELECT * FROM @IndexedIds I JOIN [dbo].[LookupDefinitions] D ON I.[Id] = D.[Id] WHERE D.[State] <> N'Hidden')
		UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END;