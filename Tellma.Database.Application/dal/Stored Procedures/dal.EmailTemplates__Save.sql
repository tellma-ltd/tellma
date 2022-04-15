CREATE PROCEDURE [dal].[EmailTemplates__Save]
	@Entities [dbo].[EmailTemplateList] READONLY,
	@Parameters [dbo].[EmailTemplateParameterList] READONLY,
	@Attachments [dbo].[EmailTemplateAttachmentList] READONLY,
	@Subscribers [dbo].[EmailTemplateSubscriberList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	-- IF any deployed templates have been modified, signal everyone to refresh their caches
	IF (EXISTS (SELECT * FROM [dbo].[EmailTemplates] WHERE [Id] IN (SELECT [Id] FROM @Entities) AND [IsDeployed] = 1)) OR (EXISTS (SELECT * FROM @Entities WHERE [IsDeployed] = 1))
		UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();

	-- IF there are changes to the schedules, signal the scheduler 
	IF (EXISTS (SELECT * FROM @Entities N WHERE N.[Id] = 0 AND N.[IsDeployed] = 1 AND N.[Trigger] = N'Automatic')) -- New matching template
		UPDATE [dbo].[Settings] SET [SchedulesVersion] = NEWID();
		
	If EXISTS (
		SELECT * FROM @Entities N 
		JOIN [dbo].[EmailTemplates] O ON N.[Id] = O.[Id] 
		WHERE 
			((N.[IsDeployed] = 1 AND N.[Trigger] = N'Automatic') AND NOT (O.[IsDeployed] = 1 AND O.[Trigger] = N'Automatic')) OR -- Wasn't matching then became matching
			((O.[IsDeployed] = 1 AND O.[Trigger] = N'Automatic') AND NOT (N.[IsDeployed] = 1 AND N.[Trigger] = N'Automatic')) OR -- Was matching then will no longer be matching
			(N.[IsDeployed] = 1 AND N.[Trigger] = N'Automatic' AND N.[Schedule] <> O.[Schedule]) OR -- The schedule column has changed
			(O.[IsError] = 1) -- A template error has been potentially fixed
	)
		UPDATE [dbo].[Settings] SET [SchedulesVersion] = NEWID();

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[EmailTemplates] AS t
		USING (
			SELECT [Index], [Id],
				[Name], 
				[Name2], 
				[Name3], 
				[Code], 
				[Description], 
				[Description2], 
				[Description3],

				[Trigger],
				[Cardinality],
				[ListExpression],
				[Schedule],
				[ConditionExpression],
				[Usage],
				[Collection],
				[DefinitionId],
				[Subject],
				[Body],
				[EmailAddress],
				[Caption],
				[IsDeployed],
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

				t.[Trigger]					= s.[Trigger],
				t.[Cardinality]				= s.[Cardinality],
				t.[ListExpression]			= s.[ListExpression],
				t.[Schedule]				= s.[Schedule],
				t.[ConditionExpression]		= s.[ConditionExpression],
				t.[Usage]					= s.[Usage],
				t.[Collection]				= s.[Collection],
				t.[DefinitionId]			= s.[DefinitionId],
				t.[Subject]					= s.[Subject],
				t.[Body]					= s.[Body],
				t.[EmailAddress]			= s.[EmailAddress],
				t.[Caption]					= s.[Caption],
				t.[IsDeployed]				= s.[IsDeployed],
				t.[MainMenuSection]			= s.[MainMenuSection],
				t.[MainMenuIcon]			= s.[MainMenuIcon],
				t.[MainMenuSortKey]			= s.[MainMenuSortKey],
				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId,
				t.[LastExecuted]			= IIF(s.[Schedule] <> t.[Schedule] OR s.[IsDeployed] <> t.[IsDeployed] OR s.[Trigger] <> t.[Trigger], @Now, t.[LastExecuted]),
				t.[IsError]					= 0
		WHEN NOT MATCHED THEN
			INSERT (
				[Name], 
				[Name2], 
				[Name3], 
				[Code], 
				[Description], 
				[Description2], 
				[Description3], 

				[Trigger],
				[Cardinality],
				[ListExpression],
				[Schedule],
				[ConditionExpression],
				[Usage],
				[Collection],
				[DefinitionId],
				[Subject],
				[Body],
				[EmailAddress],
				[Caption],
				[IsDeployed],
				[MainMenuSection],
				[MainMenuIcon],
				[MainMenuSortKey],
				[CreatedById], 
				[CreatedAt], 
				[ModifiedById], 
				[ModifiedAt],
				[LastExecuted]
			)
			VALUES (
				s.[Name], 
				s.[Name2], 
				s.[Name3], 
				s.[Code], 
				s.[Description], 
				s.[Description2], 
				s.[Description3], 
				s.[Trigger],
				s.[Cardinality],
				s.[ListExpression],
				s.[Schedule],
				s.[ConditionExpression],
				s.[Usage],
				s.[Collection],
				s.[DefinitionId],
				s.[Subject],
				s.[Body],
				s.[EmailAddress],
				s.[Caption],
				s.[IsDeployed],
				s.[MainMenuSection],
				s.[MainMenuIcon],
				s.[MainMenuSortKey],	
				@UserId, 
				@Now, 
				@UserId, 
				@Now,
				@Now
				)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	
	-- Parameters
	WITH BP AS (
		SELECT * FROM [dbo].[EmailTemplateParameters]
		WHERE [EmailTemplateId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BP AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [EmailTemplateId], L.[Key], L.[Label], L.[Label2], L.[Label3], L.[IsRequired], L.[Control], L.[ControlOptions]
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
			[Index], [EmailTemplateId], [Key], [Label], [Label2], [Label3], [IsRequired], [Control], [ControlOptions]
		)
		VALUES (
			s.[Index], s.[EmailTemplateId], s.[Key], s.[Label], s.[Label2], s.[Label3], s.[IsRequired], s.[Control], s.[ControlOptions]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;
				
	-- Attachments
	WITH BP AS (
		SELECT * FROM [dbo].[EmailTemplateAttachments]
		WHERE [EmailTemplateId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BP AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [EmailTemplateId], L.[ContextOverride], L.[DownloadNameOverride], L.[PrintingTemplateId]
		FROM @Attachments L
		JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[ContextOverride]		= s.[ContextOverride],
			t.[DownloadNameOverride] = s.[DownloadNameOverride],
			t.[PrintingTemplateId]	= s.[PrintingTemplateId]
	WHEN NOT MATCHED THEN
		INSERT (
			[Index], [EmailTemplateId], [ContextOverride], [DownloadNameOverride], [PrintingTemplateId]
		)
		VALUES (
			s.[Index], s.[EmailTemplateId], s.[ContextOverride], s.[DownloadNameOverride], s.[PrintingTemplateId]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

						
	-- Subscribers
	WITH BP AS (
		SELECT * FROM [dbo].[EmailTemplateSubscribers]
		WHERE [EmailTemplateId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BP AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [EmailTemplateId], L.[UserId]
		FROM @Subscribers L
		JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[UserId]				= s.[UserId]
	WHEN NOT MATCHED THEN
		INSERT (
			[EmailTemplateId], [UserId]
		)
		VALUES (
			s.[EmailTemplateId], s.[UserId]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	IF @ReturnIds = 1
	SELECT * FROM @IndexedIds;
END;