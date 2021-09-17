CREATE PROCEDURE [dal].[PrintingTemplates__Save]
	@Entities [dbo].[PrintingTemplateList] READONLY,
	@Parameters [dbo].[PrintingTemplateParameterList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	-- IF any deployed templates have been modified, signal everyone to refresh their caches
	IF (EXISTS (SELECT * FROM [dbo].[PrintingTemplates] WHERE [Id] IN (SELECT [Id] FROM @Entities) AND [IsDeployed] = 1)) OR (EXISTS (SELECT * FROM @Entities WHERE [IsDeployed] = 1))
		UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[PrintingTemplates] AS t
		USING (
			SELECT [Index], [Id],
				[Name], 
				[Name2], 
				[Name3], 
				[Code], 
				[Description], 
				[Description2], 
				[Description3], 
				[Context], 
				[Usage], 
				[Collection], 
				[DefinitionId], 
				[SupportsPrimaryLanguage],
				[SupportsSecondaryLanguage],
				[SupportsTernaryLanguage],
				[DownloadName],
				[Body],
				[IsDeployed]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
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
				t.[Context]					= s.[Context],
				t.[Usage]					= s.[Usage],
				t.[Collection]				= s.[Collection],
				t.[DefinitionId]			= s.[DefinitionId],
				t.[SupportsPrimaryLanguage]	= s.[SupportsPrimaryLanguage],
				t.[SupportsSecondaryLanguage]	= s.[SupportsSecondaryLanguage],
				t.[SupportsTernaryLanguage]	= s.[SupportsTernaryLanguage],
				t.[DownloadName]			= s.[DownloadName],
				t.[Body]					= s.[Body],
				t.[IsDeployed]				= s.[IsDeployed],
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
				[Context],
				[Usage], 
				[Collection], 
				[DefinitionId], 
				[SupportsPrimaryLanguage],
				[SupportsSecondaryLanguage],
				[SupportsTernaryLanguage],
				[DownloadName],
				[Body],
				[IsDeployed],
				[CreatedById], 
				[CreatedAt], 
				[ModifiedById], 
				[ModifiedAt]
				)
			VALUES (
				s.[Name], 
				s.[Name2], 
				s.[Name3], 
				s.[Code], 
				s.[Description], 
				s.[Description2], 
				s.[Description3], 
				s.[Context],
				s.[Usage], 
				s.[Collection], 
				s.[DefinitionId], 
				s.[SupportsPrimaryLanguage],
				s.[SupportsSecondaryLanguage],
				s.[SupportsTernaryLanguage],
				s.[DownloadName],
				s.[Body],
				s.[IsDeployed],
				@UserId, 
				@Now, 
				@UserId, 
				@Now
				)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	
	-- Parameters
	WITH BP AS (
		SELECT * FROM [dbo].[PrintingTemplateParameters]
		WHERE [PrintingTemplateId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BP AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [PrintingTemplateId], L.[Key], L.[Label], L.[Label2], L.[Label3], L.[IsRequired], L.[Control], L.[ControlOptions]
		FROM @Parameters L
		JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[Key]					= s.[Key],
			t.[Label]				= s.[Label],
			t.[Label2]				= s.[Label2],
			t.[Label3]				= s.[Label3],
			t.[IsRequired]			= s.[IsRequired],
			t.[Control]				= s.[Control],
			t.[ControlOptions]		= s.[ControlOptions]
	WHEN NOT MATCHED THEN
		INSERT (
			[Index], [PrintingTemplateId], [Key], [Label], [Label2], [Label3], [IsRequired], [Control], [ControlOptions]
		)
		VALUES (
			s.[Index], s.[PrintingTemplateId], s.[Key], s.[Label], s.[Label2], s.[Label3], s.[IsRequired], s.[Control], s.[ControlOptions]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	IF @ReturnIds = 1
	SELECT * FROM @IndexedIds;
END;