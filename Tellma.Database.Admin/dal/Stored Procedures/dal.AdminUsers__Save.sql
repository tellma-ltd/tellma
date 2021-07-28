CREATE PROCEDURE [dal].[AdminUsers__Save]
	@Entities [dbo].[AdminUserList] READONLY,
	@Permissions [dbo].[AdminPermissionList] READONLY,
	@UserId INT,
	@ReturnIds BIT = 0
AS
BEGIN
SET NOCOUNT ON;

	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	--DECLARE @UserId INT;

	---- This exceptional case happens during first provisioning
	--IF NOT EXISTS (SELECT * FROM dbo.[AdminUsers])
	--BEGIN
	--	SET @UserId = IDENT_CURRENT('[dbo].[AdminUsers]');
	--	EXEC [sys].[sp_set_session_context] 'UserId', @UserId;
	--END
		
	--SET @UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	
	-- Admin Users
	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[AdminUsers] AS t
		USING (
			SELECT [Index], [Id], [Email], [Name]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				--t.[Email]			= s.[Email],
				--t.[ExternalId]	    = (CASE WHEN (t.[Email] = s.[Email]) THEN t.[ExternalId] ELSE NULL END),
				t.[Name]				= s.[Name],
				t.[PermissionsVersion]	= NEWID(), -- To trigger clients to refresh cached permissions
				t.[UserSettingsVersion] = NEWID(), -- To trigger clients to refresh cached user settings
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([Name], [Email], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
			VALUES (s.[Name], s.[Email], @Now, @UserId, @Now, @UserId)
		OUTPUT s.[Index], INSERTED.[Id]
	) AS x
	OPTION (RECOMPILE);

	-- Admin Permissions
	WITH BE AS (
		SELECT * FROM [dbo].[AdminPermissions]
		WHERE [AdminUserId] IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO BE AS t
	USING (
		SELECT L.[Index], L.[Id], H.[Id] AS [AdminUserId], [View], [Action], [Criteria], [Memo]
		FROM @Permissions L
		JOIN @IndexedIds H ON L.[HeaderIndex] = H.[Index]
	) AS s ON t.[Id] = s.[Id]
	WHEN MATCHED THEN
		UPDATE SET 
			t.[View]			= s.[View], 
			t.[Action]			= s.[Action], 
			t.[Criteria]		= s.[Criteria], 
			t.[Memo]			= s.[Memo],
			t.[ModifiedAt]		= @Now,
			t.[ModifiedById]	= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([AdminUserId], [View], [Action], [Criteria], [Memo], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
		VALUES (s.[AdminUserId], s.[View], s.[Action], s.[Criteria], s.[Memo], @Now, @UserId, @Now, @UserId)
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- Sync with Directory Users
	MERGE INTO [dbo].[DirectoryUsers] As t
	USING (
		SELECT [Email] FROM [dbo].[AdminUsers] 
	) As s ON t.[Email] = s.[Email]
	WHEN MATCHED AND t.[IsAdmin] <> 1 THEN -- Existing Directory User
		UPDATE SET 
			t.[IsAdmin] = 1
	WHEN NOT MATCHED THEN -- New Directory User
		INSERT ([Email], [IsAdmin])
		VALUES (s.[Email], 1);

	-- Return the results if needed
	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;

END