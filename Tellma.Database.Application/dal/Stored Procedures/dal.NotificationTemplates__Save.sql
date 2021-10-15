CREATE PROCEDURE [dal].[NotificationTemplates__Save]
	@Entities [NotificationTemplateList] READONLY,
	@Parameters [dbo].[NotificationTemplateParameterList] READONLY,
	@Attachments [dbo].[NotificationTemplateAttachmentList] READONLY,
	@Subscribers [dbo].[NotificationTemplateSubscriberList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	-- IF any deployed templates have been modified, signal everyone to refresh their caches
	IF (EXISTS (SELECT * FROM [dbo].[NotificationTemplates] WHERE [Id] IN (SELECT [Id] FROM @Entities) AND [IsDeployed] = 1)) OR (EXISTS (SELECT * FROM @Entities WHERE [IsDeployed] = 1))
		UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[NotificationTemplates] AS t
		USING (
			SELECT [Index], [Id],
				[Name], 
				[Name2], 
				[Name3], 
				[Code], 
				[Description], 
				[Description2], 
				[Description3],

				[Channel],
				[Trigger],
				[Cardinality],
				[ListExpression],
				[Schedule],
				[ConditionExpression],
				[MaximumRenotify],
				[ErrorsEmail],
				[Usage],
				[Collection],
				[DefinitionId],
				[ReportDefinitionId],
				[Subject],
				[Body],
				[AddressExpression],
				[Caption],
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

				t.[Channel]					= s.[Channel],
				t.[Trigger]					= s.[Trigger],
				t.[Cardinality]				= s.[Cardinality],
				t.[ListExpression]			= s.[ListExpression],
				t.[Schedule]				= s.[Schedule],
				t.[ConditionExpression]		= s.[ConditionExpression],
				t.[MaximumRenotify]			= s.[MaximumRenotify],
				t.[ErrorsEmail]				= s.[ErrorsEmail],
				t.[Usage]					= s.[Usage],
				t.[Collection]				= s.[Collection],
				t.[DefinitionId]			= s.[DefinitionId],
				t.[ReportDefinitionId]		= s.[ReportDefinitionId],
				t.[Subject]					= s.[Subject],
				t.[Body]					= s.[Body],
				t.[AddressExpression]		= s.[AddressExpression],
				t.[Caption]					= s.[Caption],
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

				[Channel],
				[Trigger],
				[Cardinality],
				[ListExpression],
				[Schedule],
				[ConditionExpression],
				[MaximumRenotify],
				[ErrorsEmail],
				[Usage],
				[Collection],
				[DefinitionId],
				[ReportDefinitionId],
				[Subject],
				[Body],
				[AddressExpression],
				[Caption],
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
				s.[Channel],
				s.[Trigger],
				s.[Cardinality],
				s.[ListExpression],
				s.[Schedule],
				s.[ConditionExpression],
				s.[MaximumRenotify],
				s.[ErrorsEmail],
				s.[Usage],
				s.[Collection],
				s.[DefinitionId],
				s.[ReportDefinitionId],
				s.[Subject],
				s.[Body],
				s.[AddressExpression],
				s.[Caption],
				s.[IsDeployed],				@UserId, 
				@Now, 
				@UserId, 
				@Now
				)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;

	
	-- Parameters
	WITH BP AS (
		SELECT * FROM [dbo].[NotificationTemplateParameters]
		WHERE [NotificationTemplateId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BP AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [NotificationTemplateId], L.[Key], L.[Label], L.[Label2], L.[Label3], L.[IsRequired], L.[Control], L.[ControlOptions]
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
			[Index], [NotificationTemplateId], [Key], [Label], [Label2], [Label3], [IsRequired], [Control], [ControlOptions]
		)
		VALUES (
			s.[Index], s.[NotificationTemplateId], s.[Key], s.[Label], s.[Label2], s.[Label3], s.[IsRequired], s.[Control], s.[ControlOptions]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;
				
	-- Attachments
	WITH BP AS (
		SELECT * FROM [dbo].[NotificationTemplateAttachments]
		WHERE [NotificationTemplateId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BP AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [NotificationTemplateId], L.[ContextOverride], L.[DownloadNameOverride], L.[PrintingTemplateId]
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
			[Index], [NotificationTemplateId], [ContextOverride], [DownloadNameOverride], [PrintingTemplateId]
		)
		VALUES (
			s.[Index], s.[NotificationTemplateId], s.[ContextOverride], s.[DownloadNameOverride], s.[PrintingTemplateId]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

						
	-- Subscribers
	WITH BP AS (
		SELECT * FROM [dbo].[NotificationTemplateSubscribers]
		WHERE [NotificationTemplateId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BP AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] As [NotificationTemplateId], L.[AddressType], L.[UserId], L.[Email], L.[Phone]
		FROM @Subscribers L
		JOIN @Entities H ON L.[HeaderIndex] = H.[Index]
		JOIN @IndexedIds II ON H.[Index] = II.[Index]
	) AS s ON (t.[Id] = s.[Id])
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[AddressType]			= s.[AddressType],
			t.[UserId]				= s.[UserId],
			t.[Email]				= s.[Email],
			t.[Phone]				= s.[Phone]
	WHEN NOT MATCHED THEN
		INSERT (
			[NotificationTemplateId], [AddressType], [UserId], [Email], [Phone]
		)
		VALUES (
			s.[NotificationTemplateId], s.[AddressType], s.[UserId], s.[Email], s.[Phone]
		)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	IF @ReturnIds = 1
	SELECT * FROM @IndexedIds;
END;