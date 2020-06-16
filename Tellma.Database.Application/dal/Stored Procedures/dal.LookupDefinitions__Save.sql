CREATE PROCEDURE [dal].[LookupDefinitions__Save]
	@Entities [LookupDefinitionList] READONLY,
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
			INSERT ([Code],	[TitleSingular],	[TitleSingular2], [TitleSingular3],		[TitlePlural],	[TitlePlural2],		[TitlePlural3], [MainMenuIcon],		[MainMenuSection], [MainMenuSortKey])
			VALUES (s.[Code], s.[TitleSingular], s.[TitleSingular2], s.[TitleSingular3], s.[TitlePlural], s.[TitlePlural2], s.[TitlePlural3], s.[MainMenuIcon], s.[MainMenuSection], s.[MainMenuSortKey])
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;