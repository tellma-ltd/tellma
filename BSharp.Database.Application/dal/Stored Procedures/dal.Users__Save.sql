CREATE PROCEDURE [dal].[Users__Save]
	@Entities [dbo].[UserList] READONLY,
	@Roles [dbo].[RoleMembershipList] READONLY
AS
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	CREATE TABLE #Emails (
		[OldEmail] NVARCHAR(255) NULL,
		[NewEmail] NVARCHAR(255) NULL
	);

	INSERT INTO #Emails([OldEmail], [NewEmail])
	SELECT x.[OldEmail], x.[NewEmail]
	FROM
	(
		MERGE INTO [dbo].[Users] AS t
		USING (
			SELECT
				[Id], [Email]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				--t.[Email]			= s.[Email],
				--t.[ExternalId]	    = (CASE WHEN (t.[Email] = s.[Email]) THEN t.[ExternalId] ELSE NULL END),
				t.[PermissionsVersion] = NEWID(),
				t.[ModifiedAt]		= @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([Id], [Email])
			VALUES (s.[Id], s.[Email])
		OUTPUT INSERTED.[Email] AS [NewEmail], DELETED.[Email] AS [OldEmail]
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

	-- Return the new emails
	SELECT [NewEmail] FROM #Emails WHERE [NewEmail] IS NOT NULL AND ([OldEmail] IS NULL OR [NewEmail] <> [OldEmail]);

	-- Then return the old emails
	SELECT [OldEmail] FROM #Emails WHERE [OldEmail] IS NOT NULL AND ([NewEmail] IS NULL OR [NewEmail] <> [OldEmail]);
