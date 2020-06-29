IF NOT EXISTS(SELECT * FROM dbo.[users] WHERE [Email] = @DeployEmail) -- we need this code to run always
BEGIN
	INSERT INTO @Users ([Name],[Email])
	VALUES(N'Administrator', @DeployEmail)

	EXEC [dal].[Users__Save]
		@Entities = @Users
	
	DELETE FROM @Users;
END	

DECLARE @AdminUserId INT = (SELECT [Id] FROM dbo.[users] WHERE [Email] = @DeployEmail);
EXEC sys.sp_set_session_context 'UserId', @AdminUserId;