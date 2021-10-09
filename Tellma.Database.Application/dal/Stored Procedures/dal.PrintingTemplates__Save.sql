CREATE PROCEDURE [dal].[PrintingTemplates__Save]
	@Entities [dbo].[PrintingTemplateList] READONLY,
	@Parameters [dbo].[PrintingTemplateParameterList] READONLY,
	@Roles [dbo].[PrintingTemplateRoleList] READONLY,
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

	-- Update all users whose main menu templates have changed
	IF EXISTS (
		SELECT * FROM dbo.[Roles] R
		JOIN dbo.[PrintingTemplateRoles] DR ON DR.[RoleId] = R.[Id]
		JOIN dbo.[PrintingTemplates] D ON D.[Id] = DR.[PrintingTemplateId]
		WHERE
			D.[Id] IN (SELECT [Id] FROM @Entities) AND
			R.[IsActive] = 1 AND
			R.[IsPublic] = 1
	)
	BEGIN
		 -- If a public role is mentioned invalidate the cache for all users
		UPDATE dbo.[Users] SET [PermissionsVersion] = NEWID();
	END
	ELSE
	BEGIN
		-- Invalidate the cache for affected users only
		UPDATE U
		SET U.[PermissionsVersion] = NEWID()
		FROM dbo.[Users] U
		JOIN dbo.[RoleMemberships] RM ON U.[Id] = RM.[UserId]
		JOIN dbo.[Roles] R ON RM.[RoleId] = R.[Id]
		JOIN dbo.[PrintingTemplateRoles] DR ON DR.[RoleId] = R.[Id]
		JOIN dbo.[PrintingTemplates] D ON D.[Id] = DR.[PrintingTemplateId]
		WHERE 
			D.[Id] IN (SELECT [Id] FROM @Entities) AND 
			R.[IsActive] = 1
	END

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
				[ReportDefinitionId], 
				[SupportsPrimaryLanguage],
				[SupportsSecondaryLanguage],
				[SupportsTernaryLanguage],
				[DownloadName],
				[Body],
				[IsDeployed],
				IIF(EXISTS (SELECT 1 FROM @Roles R WHERE R.[HeaderIndex] = [Index]), 1, 0) AS [ShowInMainMenu],
				[MainMenuSection],
				[MainMenuIcon],
				[MainMenuSortKey]

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
				t.[ReportDefinitionId]		= s.[ReportDefinitionId],
				t.[SupportsPrimaryLanguage]	= s.[SupportsPrimaryLanguage],
				t.[SupportsSecondaryLanguage]	= s.[SupportsSecondaryLanguage],
				t.[SupportsTernaryLanguage]	= s.[SupportsTernaryLanguage],
				t.[DownloadName]			= s.[DownloadName],
				t.[Body]					= s.[Body],
				t.[IsDeployed]				= s.[IsDeployed],
				t.[ShowInMainMenu]			= s.[ShowInMainMenu],
				t.[MainMenuSection]			= s.[MainMenuSection],
				t.[MainMenuIcon]			= s.[MainMenuIcon],
				t.[MainMenuSortKey]			= s.[MainMenuSortKey],
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
				[ReportDefinitionId],
				[SupportsPrimaryLanguage],
				[SupportsSecondaryLanguage],
				[SupportsTernaryLanguage],
				[DownloadName],
				[Body],
				[IsDeployed],
				[ShowInMainMenu],
				[MainMenuSection],
				[MainMenuIcon],
				[MainMenuSortKey],
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
				s.[ReportDefinitionId],
				s.[SupportsPrimaryLanguage],
				s.[SupportsSecondaryLanguage],
				s.[SupportsTernaryLanguage],
				s.[DownloadName],
				s.[Body],
				s.[IsDeployed],
				s.[ShowInMainMenu],
				s.[MainMenuSection],
				s.[MainMenuIcon],
				s.[MainMenuSortKey],
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

				
	-- Roles
	WITH BM AS (
		SELECT * FROM [dbo].[PrintingTemplateRoles]
		WHERE [PrintingTemplateId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BM AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [PrintingTemplateId], L.[RoleId]
		FROM @Roles L 
		JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[RoleId]				= s.[RoleId]
	WHEN NOT MATCHED THEN
		INSERT (
			[PrintingTemplateId], [RoleId]
		)
		VALUES (
			s.[PrintingTemplateId], s.[RoleId]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Update all users whose main menu templates have changed
	IF EXISTS (
		SELECT * FROM dbo.[Roles] R
		JOIN dbo.[PrintingTemplateRoles] DR ON DR.[RoleId] = R.[Id]
		JOIN dbo.[PrintingTemplates] D ON D.[Id] = DR.[PrintingTemplateId]
		WHERE
			D.[Id] IN (SELECT [Id] FROM @IndexedIds) AND 
			D.[ShowInMainMenu] = 1 AND
			R.[IsActive] = 1 AND
			R.[IsPublic] = 1
	)
	BEGIN
		 -- If a public role is mentioned invalidate the cache for all users
		UPDATE dbo.[Users] SET [PermissionsVersion] = NEWID();
	END
	ELSE
	BEGIN
		-- Invalidate the cache for affected users only
		UPDATE U
		SET U.[PermissionsVersion] = NEWID()
		FROM dbo.[Users] U
		JOIN dbo.[RoleMemberships] RM ON U.[Id] = RM.[UserId]
		JOIN dbo.[Roles] R ON RM.[RoleId] = R.[Id]
		JOIN dbo.[PrintingTemplateRoles] DR ON DR.[RoleId] = R.[Id]
		JOIN dbo.[PrintingTemplates] D ON D.[Id] = DR.[PrintingTemplateId]
		WHERE 
			D.[Id] IN (SELECT [Id] FROM @IndexedIds) AND 
			D.[ShowInMainMenu] = 1 AND
			R.[IsActive] = 1
	END

	IF @ReturnIds = 1
	SELECT * FROM @IndexedIds;
END;