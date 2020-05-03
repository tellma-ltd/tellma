CREATE PROCEDURE [dal].[DocumentDefinitions__Save]
	@Entities dbo.[DocumentDefinitionList] READONLY,
	@DocumentDefinitionLineDefinitions [DocumentDefinitionLineDefinitionList] READONLY,
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
		MERGE [dbo].[DocumentDefinitions] AS t
		USING @Entities AS s
		ON s.[Id] = t.[Id]
		WHEN MATCHED THEN
			UPDATE SET
				t.[Code]				= s.[Code],
				t.[IsOriginalDocument]	= s.[IsOriginalDocument], 
				t.[TitleSingular]		= s.[TitleSingular],
				t.[TitleSingular2]		= s.[TitleSingular2],
				t.[TitleSingular3]		= s.[TitleSingular3],
				t.[TitlePlural]			= s.[TitlePlural],
				t.[TitlePlural2]		= s.[TitlePlural2],
				t.[TitlePlural3]		= s.[TitlePlural3],
				t.[Prefix]				= s.[Prefix],
				t.[CodeWidth]			= s.[CodeWidth],
				t.[MemoVisibility]		= s.[MemoVisibility],
				t.[ClearanceVisibility]	= s.[ClearanceVisibility],
				t.[MainMenuIcon]		= s.[MainMenuIcon],
				t.[MainMenuSection]		= s.[MainMenuSection],
				t.[MainMenuSortKey]		= s.[MainMenuSortKey]
		WHEN NOT MATCHED BY TARGET THEN
			INSERT (
				[Code], [IsOriginalDocument],
				[TitleSingular], [TitleSingular2], [TitleSingular3], [TitlePlural], [TitlePlural2], [TitlePlural3],
				[Prefix], [CodeWidth], [MemoVisibility], [ClearanceVisibility],
				[MainMenuIcon], [MainMenuSection], [MainMenuSortKey]
			) VALUES (
				s.[Code], s.[IsOriginalDocument],
				s.[TitleSingular], s.[TitleSingular2], s.[TitleSingular3], s.[TitlePlural], s.[TitlePlural2], s.[TitlePlural3],
				s.[Prefix], s.[CodeWidth], s.[MemoVisibility], s.[ClearanceVisibility],
				s.[MainMenuIcon], s.[MainMenuSection], s.[MainMenuSortKey])
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

MERGE [dbo].[DocumentDefinitionLineDefinitions] AS t
USING (
	SELECT
		DDLD.[Index],
		DDLD.[Id],
		II.[Id] AS [DocumentDefinitionId],
		DDLD.[LineDefinitionId],
		DDLD.[IsVisibleByDefault]
	FROM @Entities DD
	JOIN @IndexedIds II ON DD.[Index] = II.[Index]
	JOIN @DocumentDefinitionLineDefinitions DDLD ON DD.[Index] = DDLD.[HeaderIndex]
) AS s
ON s.Id = t.Id
WHEN MATCHED THEN
	UPDATE SET
		t.[Index]				= s.[Index],
		t.[LineDefinitionId]	= s.[LineDefinitionId],
		t.[IsVisibleByDefault]	= s.[IsVisibleByDefault],
		t.[SavedById]			= @UserId
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT (
		[Index], [DocumentDefinitionId],	[LineDefinitionId], [IsVisibleByDefault]
	) VALUES (
		[Index], s.[DocumentDefinitionId], s.[LineDefinitionId], s.[IsVisibleByDefault]
	);

IF @ReturnIds = 1
	SELECT * FROM @IndexedIds;