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
				t.[Email]			= s.[Email],
				t.[ModifiedAt]		= @Now,
				t.[ModifiedById]	= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([Id], [Email])
			VALUES (s.[Id], s.[Email])
		OUTPUT INSERTED.[Email] AS [NewEmail], DELETED.[Email] AS [OldEmail]
	) AS x
	OPTION (RECOMPILE);

	-- Role Memberships
	MERGE INTO [dbo].[RoleMemberships] AS t
    USING (
        SELECT [Id], [AgentId], [RoleId], [Memo] FROM @Roles
    ) AS s ON t.[Id] = s.[Id]
    WHEN MATCHED THEN
        UPDATE SET 
        	t.[AgentId]		    = s.[AgentId], 
        	t.[RoleId]		    = s.[RoleId],
        	t.[Memo]		    = s.[Memo],
			t.[SavedById]		= @UserId
    WHEN NOT MATCHED THEN
        INSERT ([AgentId],	[RoleId], [Memo])
        VALUES (s.[AgentId], s.[RoleId], s.[Memo]);

	-- Return the new emails
	SELECT [NewEmail] FROM #Emails WHERE [NewEmail] IS NOT NULL AND ([OldEmail] IS NULL OR [NewEmail] <> [OldEmail]);

	-- Then return the old emails
	SELECT [OldEmail] FROM #Emails WHERE [OldEmail] IS NOT NULL AND ([NewEmail] IS NULL OR [NewEmail] <> [OldEmail]);
