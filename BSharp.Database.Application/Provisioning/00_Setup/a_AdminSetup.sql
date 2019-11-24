	

IF NOT EXISTS(SELECT * FROM [dbo].[Users] WHERE [Email] = @DeployEmail)
BEGIN
	INSERT INTO dbo.Agents([Name],[AgentType], CreatedById, ModifiedById)
	VALUES (N'Banan IT', N'Organization', IDENT_CURRENT('dbo.Agents'), IDENT_CURRENT('dbo.Agents'));

	SET @AdminUserId= SCOPE_IDENTITY();
	INSERT INTO [dbo].[Users]([Id], [Email], CreatedById, ModifiedById)
	VALUES (@AdminUserId, @DeployEmail, @AdminUserId, @AdminUserId);
	
	INSERT INTO [dbo].[Roles] ([Name], [Name2], [Code], [IsPublic], [SavedById])
	VALUES (N'Administrator', N'المشرف', 'All', 0, @AdminUserId)
	SET @RoleId= SCOPE_IDENTITY();

	INSERT INTO [dbo].[Permissions] ([RoleId], [ViewId], [Action],  [SavedById])
	VALUES (@RoleId, N'All', N'All', @AdminUserId)

	INSERT INTO [dbo].[RoleMemberships] ([AgentId], [RoleId], [SavedById])
	VALUES								(@AdminUserId, @RoleId, @AdminUserId)
END
-- Set the user session context
SELECT @AdminUserId = [Id] FROM dbo.[Users] WHERE [Email] = @DeployEmail;
EXEC master.sys.sp_set_session_context 'UserId', @AdminUserId;

IF NOT EXISTS(SELECT * FROM dbo.ResourceDefinitions WHERE [Id] = N'monetary-resources')
	INSERT INTO dbo.ResourceDefinitions([Id])
	VALUES(N'monetary-resources');