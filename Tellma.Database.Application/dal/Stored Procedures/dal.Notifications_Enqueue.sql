CREATE PROCEDURE [dal].[Notifications_Enqueue]
	@ExpiryInSeconds					INT,
	@Emails dbo.[EmailList] READONLY,
	@EmailAttachments dbo.[EmailAttachmentList] READONLY,
	@Messages dbo.[MessageList]	READONLY,
	-- @PushNotifications dbo.[PushNotificationList] READONLY
	@TemplateId	INT,
	@EntityId	INT,
	@Caption	NVARCHAR(1024),
	@CreatedById	INT,
	@QueueEmails						BIT OUTPUT,
	@QueueMessages						BIT OUTPUT,
	@QueuePushNotifications				BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @TooOld DATETIMEOFFSET(7) = DATEADD(second, -@ExpiryInSeconds, @Now);

	SET @QueueEmails = CASE 
		WHEN EXISTS (SELECT * FROM @Emails WHERE [State] >= 0) AND EXISTS (SELECT * FROM dbo.[Emails] WHERE [State] = 0 OR ([State] = 1 AND [StateSince] < @TooOld)) THEN 0 
		ELSE 1 -- This means either the given email list contains no valid emails that can be queued, or the table contains no NEW or stale PENDING emails
	END;

	SET @QueueMessages = CASE 
		WHEN EXISTS (SELECT * FROM @Messages WHERE [State] >= 0) AND EXISTS (SELECT * FROM dbo.[Messages] WHERE [State] = 0 OR ([State] = 1 AND [StateSince] < @TooOld)) THEN 0 
		ELSE 1 -- This means either the given message list contains no valid messages that can be queued, or the table contains no NEW or stale PENDING messages
	END;

	SET @QueuePushNotifications = 1; -- TODO
	
	DECLARE @EmailCommandId INT = NULL;
	DECLARE @MessageCommandId INT = NULL;
	IF @TemplateId IS NOT NULL
	BEGIN
		IF (EXISTS (SELECT * FROM @Emails))
		BEGIN
			DECLARE @EmailCommandIds TABLE ([Id] INT)

			INSERT INTO [dbo].[NotificationCommands] ([TemplateId], [EntityId], [Caption], [CreatedById], [CreatedAt])
			OUTPUT INSERTED.[Id] INTO @EmailCommandIds([Id])
			VALUES (@TemplateId, @EntityId, @Caption, @CreatedById, @Now);

			SET @EmailCommandId = (SELECT [Id] FROM @EmailCommandIds);
		END;
		
		IF (EXISTS (SELECT * FROM @Messages))
		BEGIN
			-- Insert Message Command
			DECLARE @MessageCommandIds TABLE ([Id] INT)

			INSERT INTO [dbo].[MessageCommands] ([TemplateId], [EntityId], [Caption], [CreatedById], [CreatedAt])
			OUTPUT INSERTED.[Id] INTO @MessageCommandIds([Id])
			VALUES (@TemplateId, @EntityId, @Caption, @CreatedById, @Now);

			SET @MessageCommandId = (SELECT [Id] FROM @MessageCommandIds);
		END;
	END;

	-- Insert emails
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	
	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Emails] AS t
		USING (
			SELECT 
				[Index],
				[To], 
				[Cc], 
				[Bcc], 
				[Subject], 
				[BodyBlobId], 
				CASE 
					WHEN [State] < 0 THEN -1 
					WHEN [State] >= 0 AND @QueueEmails = 1 THEN 1 
					ELSE 0 
				END AS [State], 
				[ErrorMessage]
			FROM @Emails
		) AS s ON (1 = 0) -- TODO: Find a less hacky way to get the output?
		WHEN NOT MATCHED THEN
			INSERT ([To], [Cc], [Bcc], [Subject], [BodyBlobId], [State], [ErrorMessage], [StateSince], [CommandId])
			Values (s.[To], s.[Cc], s.[Bcc], s.[Subject], s.[BodyBlobId], s.[State], s.[ErrorMessage], @Now, @EmailCommandId)
		OUTPUT s.[Index], inserted.[Id] -- We need this output
	) AS x;

	INSERT INTO [dbo].[EmailAttachments] ([Index], [EmailId], [Name], [ContentBlobId])
	SELECT A.[Index], E.[Id], A.[Name], A.[ContentBlobId]
	FROM @EmailAttachments A JOIN @IndexedIds E ON A.[HeaderIndex] = E.[Index]

	-- Return the indices
	SELECT [Index], [Id] FROM @IndexedIds;

	-- Insert Messages
	MERGE INTO [dbo].[Messages] AS t
	USING (
		SELECT 
			[Index],
			[PhoneNumber], 
			[Content], 
			CASE 
				WHEN [State] < 0 THEN -1 
				WHEN [State] >= 0 AND @QueueMessages = 1 THEN 1 
				ELSE 0 
			END AS [State], 
			[ErrorMessage]
		FROM @Messages
	) AS s ON (1 = 0) -- TODO: Find a less hacky way?
	WHEN NOT MATCHED THEN
		INSERT ([PhoneNumber], [Content], [State], [ErrorMessage], [StateSince], [CommandId])
		VALUES (s.[PhoneNumber], s.[Content], s.[State], s.[ErrorMessage], @Now, @MessageCommandId)
	OUTPUT s.[Index], inserted.[Id];
	
	-- Insert push notifications
	-- TODO
END
