CREATE PROCEDURE [dal].[Users__Save]
	@Entities [dbo].[UserList] READONLY,
	@Roles [dbo].[RoleMembershipList] READONLY,
	@ReturnIds BIT = 0
AS
BEGIN
SET NOCOUNT ON;

	DECLARE @IndexedIds [dbo].[IndexedIdList], @DeletedImageIds [dbo].[StringList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT;

	-- This exceptional case happens during first provisioning
	IF NOT EXISTS (SELECT * FROM dbo.[Users])
	BEGIN
		SET @UserId = IDENT_CURRENT('[dbo].[Users]');
		EXEC sys.sp_set_session_context 'UserId', @UserId;
	END
		
	SET @UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	
	-- Entities whose ImageIds will be updated: capture their old ImageIds first (if any) so C# can delete them from blob storage
	INSERT INTO @DeletedImageIds ([Id])
	SELECT [ImageId] FROM dbo.[Users] E
	WHERE E.[ImageId] IS NOT NULL 
		AND E.[Id] IN (SELECT [Id] FROM @Entities WHERE [ImageId] IS NULL OR [ImageId] <> N'(Unchanged)');

	
	-- Users
	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Users] AS t
		USING (
			SELECT 
				[Index], 
				[Id], 
				[Email], 
				[Name], 
				[Name2], 
				[Name3], 
				[PreferredLanguage], 
				[PreferredCalendar], 
				[ContactEmail], 
				[ContactMobile], 
				[NormalizedContactMobile], 
				[PreferredChannel], 
				[EmailNewInboxItem], 
				[SmsNewInboxItem], 
				[PushNewInboxItem],
				[ImageId]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				--t.[Email]			= s.[Email],
				--t.[ExternalId]	    = (CASE WHEN (t.[Email] = s.[Email]) THEN t.[ExternalId] ELSE NULL END),
				t.[Name]				= s.[Name],
				t.[Name2]				= s.[Name2],
				t.[Name3]				= s.[Name3],
				t.[PreferredLanguage]	= s.[PreferredLanguage],
				t.[PreferredCalendar]	= s.[PreferredCalendar],
				t.[ContactEmail]		= s.[ContactEmail],
				t.[ContactMobile]		= s.[ContactMobile],
				t.[NormalizedContactMobile] = s.[NormalizedContactMobile],
				t.[PreferredChannel]	= s.[PreferredChannel],
				t.[EmailNewInboxItem]	= s.[EmailNewInboxItem],
				t.[SmsNewInboxItem]		= s.[SmsNewInboxItem],
				t.[PushNewInboxItem]	= s.[PushNewInboxItem],
				t.[ImageId]					= IIF(s.[ImageId] = N'(Unchanged)', t.[ImageId], s.[ImageId]),


				t.[PermissionsVersion]	= NEWID(), -- To trigger clients to refresh cached permissions
				t.[UserSettingsVersion] = NEWID(), -- To trigger clients to refresh cached user settings
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[Name], 
				[Name2], 
				[Name3], 
				[Email], 
				[PreferredLanguage], 
				[PreferredCalendar], 
				[ContactEmail], 
				[ContactMobile], 
				[NormalizedContactMobile], 
				[PreferredChannel], 
				[EmailNewInboxItem], 
				[SmsNewInboxItem], 
				[PushNewInboxItem],
				[ImageId])
			VALUES (
				s.[Name], 
				s.[Name2], 
				s.[Name3], 
				s.[Email], 
				s.[PreferredLanguage], 
				s.[PreferredCalendar], 
				s.[ContactEmail], 
				s.[ContactMobile],
				s.[NormalizedContactMobile],
				s.[PreferredChannel], 
				s.[EmailNewInboxItem], 
				s.[SmsNewInboxItem], 
				s.[PushNewInboxItem],
				IIF(s.[ImageId] = N'(Unchanged)', NULL, s.[ImageId])
				)
		OUTPUT s.[Index], INSERTED.[Id]
	) AS x
	OPTION (RECOMPILE);

	-- Role Memberships
	WITH BE AS (
		SELECT * FROM [dbo].[RoleMemberships]
		WHERE [UserId] IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO BE AS t
	USING (
		SELECT L.[Index], L.[Id], H.[Id] AS [UserId], [RoleId], [Memo]
		FROM @Roles L
		JOIN @IndexedIds H ON L.[HeaderIndex] = H.[Index]
	) AS s ON t.[Id] = s.[Id]
	WHEN MATCHED THEN
		UPDATE SET 
			t.[RoleId]		= s.[RoleId], 
			t.[Memo]		= s.[Memo],
			t.[SavedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([RoleId],	[UserId],	[Memo])
		VALUES (s.[RoleId], s.[UserId], s.[Memo])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Return overwritten Image Ids, so C# can delete them from Blob Storage
	SELECT [Id] FROM @DeletedImageIds;

	-- Return the results if needed
	IF @ReturnIds = 1
	SELECT * FROM @IndexedIds;

END