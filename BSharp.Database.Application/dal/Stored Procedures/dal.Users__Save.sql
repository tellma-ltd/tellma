CREATE PROCEDURE [dal].[Users__Save]
	@Entities [dbo].[UserList] READONLY,
	@ImageIds [IndexedImageIdList] READONLY, -- Index, ImageId
	@Roles [dbo].[RoleMembershipList] READONLY,
	@ReturnIds BIT = 0
AS
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	CREATE TABLE #IndexedIdsAndEmails (
		[Index]		INT,
		[Id]		INT,
		[OldEmail]	NVARCHAR(255) NULL
	);

	INSERT INTO #IndexedIdsAndEmails([Index], [Id], [OldEmail])
	SELECT x.[Index], x.[Id], x.[OldEmail]
	FROM
	(
		MERGE INTO [dbo].[Users] AS t
		USING (
			SELECT [Index], [Id], [Email], [Name], [Name2], [Name3]
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
				t.[PermissionsVersion]	= NEWID(),
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([Name], [Name2], [Name3], [Email])
			VALUES (s.[Name], s.[Name2], s.[Name3], s.[Email])
		OUTPUT s.[Index], INSERTED.[Id], DELETED.[Email] AS [OldEmail]
	) AS x
	OPTION (RECOMPILE);

	-- Role Memberships
	WITH BE AS (
		SELECT * FROM [dbo].[RoleMemberships]
		WHERE [UserId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BE AS t
	USING (
		SELECT L.[Id], [UserId], [RoleId], [Memo]
		FROM @Roles L
		JOIN @Entities H ON L.[UserId] = H.[Id]
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

	UPDATE A --dbo.Agents
	SET A.ImageId = L.ImageId
	FROM dbo.Agents A
	JOIN #IndexedIdsAndEmails II ON A.Id = II.[Id]
	JOIN @ImageIds L ON II.[Index] = L.[Index]
	
	-- To trigger clients to refresh cached settings
	UPDATE [dbo].[Users] SET [UserSettingsVersion] = NEWID()
	WHERE [Id] IN (SELECT [Id] FROM #IndexedIdsAndEmails)

	-- Return the new emails
	SELECT [NewEmail] FROM #IndexedIdsAndEmails WHERE [NewEmail] IS NOT NULL AND ([OldEmail] IS NULL OR [NewEmail] <> [OldEmail]);

	-- Then return the old emails
	SELECT [OldEmail] FROM #IndexedIdsAndEmails WHERE [OldEmail] IS NOT NULL AND ([NewEmail] IS NULL OR [NewEmail] <> [OldEmail]);
