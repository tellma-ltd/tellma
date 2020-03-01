CREATE PROCEDURE [dal].[DocumentDefinitions__Save]
	@Entities dbo.[DocumentDefinitionList] READONLY,
	@DocumentDefinitionLineDefinitions [DocumentDefinitionLineDefinitionList] READONLY
AS
DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
MERGE [dbo].[DocumentDefinitions] AS t
USING @Entities AS s
ON s.Id = t.Id
WHEN MATCHED THEN
	UPDATE SET
		t.[IsOriginalDocument]	= s.[IsOriginalDocument], 
		t.[TitleSingular]		= s.[TitleSingular],
		t.[TitleSingular2]		= s.[TitleSingular2],
		t.[TitleSingular3]		= s.[TitleSingular3],
		t.[TitlePlural]			= s.[TitlePlural],
		t.[TitlePlural2]		= s.[TitlePlural2],
		t.[TitlePlural3]		= s.[TitlePlural3],
		t.[Prefix]				= s.[Prefix],
		t.[CodeWidth]			= s.[CodeWidth],
		t.[AgentDefinitionList]	= s.[AgentDefinitionList]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT (
		[Id], [IsOriginalDocument], [TitleSingular], [TitleSingular2], [TitleSingular3], [TitlePlural], [TitlePlural2], [TitlePlural3],
		[Prefix], [CodeWidth], [AgentDefinitionList]
	) VALUES (
		s.[Id], s.[IsOriginalDocument], s.[TitleSingular], s.[TitleSingular2], s.[TitleSingular3], s.[TitlePlural], s.[TitlePlural2], s.[TitlePlural3],
		s.[Prefix], s.[CodeWidth], s.[AgentDefinitionList]
	);

MERGE [dbo].[DocumentDefinitionLineDefinitions] AS t
USING (
	SELECT
		DDLD.[Index],
		DDLD.[Id],
		DD.[Id] AS [DocumentDefinitionId],
		DDLD.[LineDefinitionId],
		DDLD.[IsVisibleByDefault]
	FROM @Entities DD
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
		[Index], [DocumentDefinitionId],		[LineDefinitionId], [IsVisibleByDefault]
	) VALUES (
		[Index], s.[DocumentDefinitionId], s.[LineDefinitionId], s.[IsVisibleByDefault]
	);