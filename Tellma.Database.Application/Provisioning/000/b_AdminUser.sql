	INSERT INTO @Users ([Name],[Email])
	VALUES(N'Administrator', @DeployEmail)

	EXEC [dal].[Users__Save]
		@Entities = @Users
	DELETE FROM @Users;

	DECLARE @AdminUserId INT = (SELECT [Id] FROM dbo.[users] WHERE [Email] = @DeployEmail);
	EXEC sys.sp_set_session_context 'UserId', @AdminUserId;