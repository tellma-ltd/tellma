IF NOT EXISTS(SELECT * FROM [dbo].[Users] WHERE [Email] = @DeployEmail)
BEGIN
	DECLARE @Users dbo.UserList;
	INSERT INTO @Users
	([Name],			[Name2],	[Email]) VALUES
	(N'Administrator',	N'المشرف',	@DeployEmail);

	EXEC [dal].[Users__Save]
		@Entities = @Users
END

DECLARE @AdminUserId INT = (SELECT[Id] FROM dbo.[Users] WHERE [Email] = @DeployEmail);
--EXEC master.sys.sp_set_session_context 'UserId', @AdminUserId;

IF  NOT EXISTS(SELECT * FROM [dbo].[Roles] WHERE [Code] = N'All')
BEGIN
	DECLARE @Roles dbo.RoleList,@Members [dbo].[RoleMembershipList], @Permissions dbo.PermissionList;
	
	INSERT INTO @Roles ([Name],[Name2],[Code]) VALUES	(N'Administrator', N'المشرف', 'All');
	INSERT INTO @Members ([UserId])	VALUES (@AdminUserId);
	INSERT INTO @Permissions ([View],	[Action]) VALUES (N'all', N'All');

	EXEC [dal].[Roles__Save]
		@Entities = @Roles,
		@Members = @Members,
		@Permissions = @Permissions;
END;