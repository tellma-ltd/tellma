CREATE PROCEDURE [dal].[MarkupTemplates__Save]
	@Entities [MarkupTemplateList] READONLY,
	@ReturnIds BIT = 0
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[MarkupTemplates] AS t
		USING (
			SELECT [Index], [Id], --[OperatingSegmentId],
				[Name], 
				[Name2], 
				[Name3], 
				[Code], 
				[Description], 
				[Description2], 
				[Description3], 
				[Usage], 
				[Collection], 
				[DefinitionId], 
				[MarkupLanguage],
				[SupportsPrimaryLanguage],
				[SupportsSecondaryLanguage],
				[SupportsTernaryLanguage],
				[DownloadName],
				[Body]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED
		THEN
			UPDATE SET
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[Code]					= s.[Code],
				t.[Description]				= s.[Description],
				t.[Description2]			= s.[Description2],
				t.[Description3]			= s.[Description3],
				t.[Usage]					= s.[Usage],
				t.[Collection]				= s.[Collection],
				t.[DefinitionId]			= s.[DefinitionId],
				t.[MarkupLanguage]			= s.[MarkupLanguage],
				t.[SupportsPrimaryLanguage]	= s.[SupportsPrimaryLanguage],
				t.[SupportsSecondaryLanguage]	= s.[SupportsSecondaryLanguage],
				t.[SupportsTernaryLanguage]	= s.[SupportsTernaryLanguage],
				t.[DownloadName]			= s.[DownloadName],
				t.[Body]					= s.[Body],

				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[Name], 
				[Name2], 
				[Name3], 
				[Code], 
				[Description], 
				[Description2], 
				[Description3], 
				[Usage], 
				[Collection], 
				[DefinitionId], 
				[MarkupLanguage],
				[SupportsPrimaryLanguage],
				[SupportsSecondaryLanguage],
				[SupportsTernaryLanguage],
				[DownloadName],
				[Body]
				)
			VALUES (
				s.[Name], 
				s.[Name2], 
				s.[Name3], 
				s.[Code], 
				s.[Description], 
				s.[Description2], 
				s.[Description3], 
				s.[Usage], 
				s.[Collection], 
				s.[DefinitionId], 
				s.[MarkupLanguage],
				s.[SupportsPrimaryLanguage],
				s.[SupportsSecondaryLanguage],
				s.[SupportsTernaryLanguage],
				s.[DownloadName],
				s.[Body]
				)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
	SELECT * FROM @IndexedIds;
END