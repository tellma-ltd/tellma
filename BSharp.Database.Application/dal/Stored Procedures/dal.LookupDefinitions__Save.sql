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
			SELECT [Index], [Id], [TitleSingular], [TitleSingular2], [TitleSingular3],
				[TitlePlural], [TitlePlural2], [TitlePlural3], [MainMenuIcon],
				[MainMenuSection], [MainMenuSortKey]
			FROM @Entities 
		) AS s ON (t.Id = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[TitleSingular]			= s.[TitleSingular],
				t.[TitleSingular2]			= s.[TitleSingular2],
				t.[TitleSingular3]			= s.[TitleSingular3],
				t.[TitlePlural]				= s.[TitlePlural],
				t.[TitlePlural2]			= s.[TitlePlural2],
				t.[TitlePlural3]			= s.[TitlePlural3],
				t.[MainMenuIcon]			= s.[MainMenuIcon],
				t.[MainMenuSection]			= s.[MainMenuSection],
				t.[MainMenuSortKey]			= s.[MainMenuSortKey],
				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([TitleSingular], [TitleSingular2], [TitleSingular3], [TitlePlural], [TitlePlural2], [TitlePlural3], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey])
			VALUES (s.[TitleSingular], s.[TitleSingular2], s.[TitleSingular3], s.[TitlePlural], s.[TitlePlural2], s.[TitlePlural3], s.[MainMenuIcon], s.[MainMenuSection], s.[MainMenuSortKey])
		OUTPUT s.[Index], inserted.[Id]
	) AS x
	OPTION (RECOMPILE);

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;

		/*

	[Index]						INT	PRIMARY KEY,
	[Id]						NVARCHAR (50) NOT NULL PRIMARY KEY,
	[TitleSingular]				NVARCHAR (255),
	[TitleSingular2]			NVARCHAR (255),
	[TitleSingular3]			NVARCHAR (255),
	[TitlePlural]				NVARCHAR (255),
	[TitlePlural2]				NVARCHAR (255),
	[TitlePlural3]				NVARCHAR (255),
	[MainMenuIcon]				NVARCHAR (50),
	[MainMenuSection]			NVARCHAR (50),			-- Required when the state is "Deployed"
	[MainMenuSortKey]			DECIMAL (9,4)

	*/