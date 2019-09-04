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
				[Index], [Id], [Email]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				--t.[Email]			= s.[Email],
				--t.[ExternalId]	    = (CASE WHEN (t.[Email] = s.[Email]) THEN t.[ExternalId] ELSE NULL END),
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
		WHERE [RoleId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE INTO BE AS t
	USING (
		SELECT L.[Index], L.[Id], II.[Id] AS [RoleId], [AgentId], [Memo]
		FROM @Roles L
		JOIN @Entities II ON L.[HeaderIndex] = II.[Index]
	) AS s ON t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET 
			t.[AgentId]		= s.[AgentId], 
			t.[Memo]		= s.[Memo],
			t.[SavedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([RoleId],	[AgentId],	[Memo])
		VALUES (s.[RoleId], s.[AgentId], s.[Memo])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Return the new emails
	SELECT [NewEmail] FROM #Emails WHERE [NewEmail] IS NOT NULL AND ([OldEmail] IS NULL OR [NewEmail] <> [OldEmail]);

	-- Then return the old emails
	SELECT [OldEmail] FROM #Emails WHERE [OldEmail] IS NOT NULL AND ([NewEmail] IS NULL OR [NewEmail] <> [OldEmail]);
