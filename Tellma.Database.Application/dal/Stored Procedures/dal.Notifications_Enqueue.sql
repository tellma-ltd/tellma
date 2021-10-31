CREATE PROCEDURE [dal].[Notifications_Enqueue]
	@ExpiryInSeconds					INT,
	@Emails dbo.[EmailList] READONLY,
	@EmailAttachments dbo.[EmailAttachmentList] READONLY,
	@SmsMessages dbo.[SmsMessageList]	READONLY,
	-- @PushNotifications dbo.[PushNotificationList] READONLY
	@QueueEmails						BIT OUTPUT,
	@QueueSmsMessages					BIT OUTPUT,
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

	SET @QueueSmsMessages = CASE 
		WHEN EXISTS (SELECT * FROM @SmsMessages WHERE [State] >= 0) AND EXISTS (SELECT * FROM dbo.[SmsMessages] WHERE [State] = 0 OR ([State] = 1 AND [StateSince] < @TooOld)) THEN 0 
		ELSE 1 -- This means either the given SMS list contains no valid SMSes that can be queued, or the table contains no NEW or stale PENDING SMSes
	END;

	SET @QueuePushNotifications = 1; -- TODO

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
			INSERT ([To], [Cc], [Bcc], [Subject], [BodyBlobId], [State], [ErrorMessage], [StateSince])
			Values (s.[To], s.[Cc], s.[Bcc], s.[Subject], s.[BodyBlobId], s.[State], s.[ErrorMessage], @Now)
		OUTPUT s.[Index], inserted.[Id] -- We need this output
	) AS x;

	INSERT INTO [dbo].[EmailAttachments] ([Index], [EmailId], [Name], [ContentBlobId])
	SELECT A.[Index], E.[Id], A.[Name], A.[ContentBlobId]
	FROM @EmailAttachments A JOIN @IndexedIds E ON A.[HeaderIndex] = E.[Index]

	-- Return the indices
	SELECT [Index], [Id] FROM @IndexedIds;

	-- Insert SMS messages
	MERGE INTO [dbo].[SmsMessages] AS t
	USING (
		SELECT 
			[Index],
			[ToPhoneNumber], 
			[Message], 
			CASE 
				WHEN [State] < 0 THEN -1 
				WHEN [State] >= 0 AND @QueueSmsMessages = 1 THEN 1 
				ELSE 0 
			END AS [State], 
			[ErrorMessage]
		FROM @SmsMessages
	) AS s ON (1 = 0) -- TODO: Find a less hacky way?
	WHEN NOT MATCHED THEN
		INSERT ([ToPhoneNumber], [Message], [State], [ErrorMessage], [StateSince])
		VALUES (s.[ToPhoneNumber], s.[Message], s.[State], s.[ErrorMessage], @Now)
	OUTPUT s.[Index], inserted.[Id];
	
	-- Insert push notifications
	-- TODO
END
