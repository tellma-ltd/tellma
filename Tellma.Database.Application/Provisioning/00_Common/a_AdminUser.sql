IF NOT EXISTS(SELECT * FROM [dbo].[Users] WHERE [Email] = @DeployEmail)
BEGIN
	DECLARE @Users dbo.UserList;

	IF @DB = N'100' -- ACME, USD, en/ar/zh playground
	BEGIN
		INSERT INTO @Users
		([Name],			[Name2],	[Name3], [Email]) VALUES
		(N'Administrator',	N'المشرف',	N'管理员', @DeployEmail);
	END
	ELSE IF @DB = N'101' -- Banan SD, USD, en
	BEGIN
		INSERT INTO @Users
		([Name],			[Email]) VALUES
		(N'Administrator',	@DeployEmail);
	END
	ELSE IF @DB = N'102' -- Banan ET, ETB, en
	BEGIN
		INSERT INTO @Users
		([Name],			[Email]) VALUES
		(N'Administrator',	@DeployEmail);
	END
	ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh car service
	BEGIN
		INSERT INTO @Users
		([Name],			[Name2], [Email]) VALUES
		(N'Administrator',	N'管理员', @DeployEmail);
	END
	ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am manyfacturing and sales
	BEGIN
		INSERT INTO @Users
		([Name],			[Name2], [Email]) VALUES
		(N'Administrator',	N'አስተዳዳሪ', @DeployEmail);
	END
	ELSE IF @DB = N'105' -- Simpex, SAR, en/ar trading
	BEGIN
		INSERT INTO @Users
		([Name],			[Name2],	[Email]) VALUES
		(N'Administrator',	N'المشرف',	@DeployEmail);
	END

	EXEC [dal].[Users__Save]
		@Entities = @Users
END

DECLARE @AdminUserId INT = (SELECT[Id] FROM dbo.[Users] WHERE [Email] = @DeployEmail);
--EXEC master.sys.sp_set_session_context 'UserId', @AdminUserId;

IF  NOT EXISTS(SELECT * FROM [dbo].[Roles] WHERE [Code] = N'All')
BEGIN
	DECLARE @Roles dbo.RoleList,@Members [dbo].[RoleMembershipList], @Permissions dbo.PermissionList;
	IF @DB = N'100' -- ACME, USD, en/ar/zh playground
	BEGIN
		INSERT INTO @Roles
		([Name],[Name2],[Name3],[Code]) VALUES
		(N'Administrator', N'المشرف', N'管理员', 'All');
	END
	ELSE IF @DB = N'101' -- Banan SD, USD, en
	BEGIN
		INSERT INTO @Roles
		([Name],[Code]) VALUES
		(N'Administrator', 'All');
	END
	ELSE IF @DB = N'102' -- Banan ET, ETB, en
	BEGIN
		INSERT INTO @Roles
		([Name],[Code]) VALUES
		(N'Administrator', 'All');
	END
	ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh car service
	BEGIN
		INSERT INTO @Roles
		([Name],			[Name2], [Code]) VALUES
		(N'Administrator',	N'管理员', 'All');
	END
	ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am manyfacturing and sales
	BEGIN
		INSERT INTO @Roles
		([Name],			[Name2], [Code]) VALUES
		(N'Administrator',	N'አስተዳዳሪ', 'All');
	END
	ELSE IF @DB = N'105' -- Simpex, SAR, en/ar trading
	BEGIN
		INSERT INTO @Roles
		([Name],[Name2],[Code]) VALUES
		(N'Administrator', N'المشرف', 'All');
	END
	  	  
	INSERT INTO @Members ([UserId])	VALUES (@AdminUserId);
	INSERT INTO @Permissions ([View],	[Action]) VALUES (N'all', N'All');

	EXEC [dal].[Roles__Save]
		@Entities = @Roles,
		@Members = @Members,
		@Permissions = @Permissions;
END;