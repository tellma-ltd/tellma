-- Roles
SET IDENTITY_INSERT [dbo].[Roles] ON; 
MERGE INTO [dbo].[Roles] AS t
USING (
	VALUES
	(@AdminRoleId, N'Administrator', N'المشرف', 0, N'Administrator'),
	(@ReaderRoleId, N'Reader', N'مطلع', 0, N'Reader'),
	(@ConsultantRoleId, N'Consultant', N'مستشار', 0, N'Consultant')
) AS s([Id], [Name], [Name2], [IsPublic], [Code]) 
ON (t.[Id] = s.[Id])
WHEN MATCHED 
THEN
	UPDATE SET
		t.[Name]			= s.[Name],
		t.[Name2]			= s.[Name2],
		t.[IsPublic]		= s.[IsPublic],
		t.[Code]			= s.[Code],
		t.[SavedById]		= @AdminId,
		t.[IsActive]			= 1
WHEN NOT MATCHED THEN
	INSERT (
		[Id], [Name], [Name2], [IsPublic], [Code], [SavedById]
	)
	VALUES (
		s.[Id], s.[Name], s.[Name2], s.[IsPublic], s.[Code], @AdminId
	);

SET IDENTITY_INSERT [dbo].[Roles] OFF;

-- Memberships
SET IDENTITY_INSERT [dbo].[RoleMemberships] ON; 
MERGE INTO [dbo].[RoleMemberships] AS t
USING (
	VALUES
		(1, @AdminRoleId, @AdminId, Null)
) AS s([Id], [RoleId], [UserId], [Memo]) 
ON (t.[Id] = s.[Id])
WHEN MATCHED THEN
	UPDATE SET 
		t.[RoleId]		= s.[RoleId],
		t.[UserId]		= s.[UserId], 
		t.[Memo]		= s.[Memo],
		t.[SavedById]	= @AdminId
WHEN NOT MATCHED THEN
	INSERT ([Id], [RoleId],	[UserId], [Memo], [SavedById])
	VALUES (s.[Id], s.[RoleId], s.[UserId], s.[Memo], @AdminId)
WHEN NOT MATCHED BY SOURCE THEN
	DELETE;

SET IDENTITY_INSERT [dbo].[RoleMemberships] OFF; 

-- Permissions
SET IDENTITY_INSERT [dbo].[Permissions] ON; 
MERGE INTO [dbo].[Permissions] AS t
USING (
	VALUES
	(1, @AdminRoleId, N'users', N'All'),
	(2, @AdminRoleId, N'roles', N'All'),
	(3, @ReaderRoleId, N'all', N'Read'),
	(4, @ConsultantRoleId, N'accounts', N'Read')
) AS s([Id], [RoleId], [View], [Action])
ON (t.[Id] = s.[Id])
WHEN MATCHED THEN
	UPDATE SET
		t.[RoleId]		= s.[RoleId],
		t.[View]		= s.[View], 
		t.[Action]		= s.[Action],
		t.[Criteria]	= NULL,
		t.[Memo]		= NULL,
		t.[SavedById]	= @AdminId
WHEN NOT MATCHED THEN
	INSERT ([Id], [RoleId],	[View],	[Action], [SavedById])
	VALUES (s.[Id], s.[RoleId], s.[View], s.[Action], @AdminId)
WHEN NOT MATCHED BY SOURCE THEN
	DELETE;
	
SET IDENTITY_INSERT [dbo].[Permissions] OFF; 

UPDATE [dbo].[Roles] SET [IsActive] = 0 WHERE [Id] = @ConsultantRoleId; -- Inactive