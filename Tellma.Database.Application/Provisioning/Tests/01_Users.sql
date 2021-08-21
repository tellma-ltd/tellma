SET IDENTITY_INSERT [dbo].[Users] ON; 

MERGE INTO [dbo].[Users] AS t
USING (
	VALUES 
	(@AdminId, N'Administrator', N'المشرف', @AdminEmail),
	(@PeterUserId, N'Peter', N'بيتر', N'peter@tellma.com'),
	(@SarahUserId, N'Sarah', N'سارة', N'sarah@tellma.com'),
	(@LouayUserId, N'Louay', N'لؤي', N'louay@tellma.com'),
	(@LucyUserId, N'Lucy', N'لوسي', N'lucy@tellma.com')
) AS s([Id], [Name], [Name2], [Email]) 
ON (t.[Id] = s.[Id])
WHEN MATCHED 
THEN
	UPDATE SET 
		t.[Name]				= s.[Name],
		t.[Name2]				= s.[Name2],
		t.[Email]				= s.[Email],
		t.[CreatedAt]			= @Now,
		t.[CreatedById]			= @AdminId,
		t.[ModifiedAt]			= @Now,
		t.[ModifiedById]		= @AdminId,
		t.[UserSettingsVersion]	= @UserSettingsVersion,
		t.[PermissionsVersion]	= @PermissionsVersion,
		t.[PreferredLanguage]	= NULL,
		t.[PreferredCalendar]	= NULL,
		t.[ExternalId]			= NULL,
		t.[InvitedAt]			= NULL,
		t.[IsActive]			= 1
WHEN NOT MATCHED THEN
	INSERT (
		[Id],
		[Name], 
		[Name2], 
		[Email],
		[IsService],
		[CreatedById],
		[CreatedAt],
		[ModifiedById],
		[ModifiedAt],
		[UserSettingsVersion],
		[PermissionsVersion])
	VALUES (
		s.[Id],
		s.[Name], 
		s.[Name2], 
		s.[Email],
		0,
		@AdminId, 
		@Now, 
		@AdminId,
		@Now,
		N'70A15FBB-DB79-4A70-8765-73446418EF16',
		N'D5E9A3CB-94D0-4D78-B63C-3ECA4923C8B4');

SET IDENTITY_INSERT [dbo].[Users] OFF;

UPDATE [dbo].[Users] SET [InvitedAt] = @Now, [ExternalId] = @AdminExternalId WHERE [Id] = @AdminId; -- Admin is built-in
UPDATE [dbo].[Users] SET [InvitedAt] = @Now, [ExternalId] = N'14070055-83E6-4DF8-A59A-E7412BB85122' WHERE [Id] = @PeterUserId; -- Peter is member
UPDATE [dbo].[Users] SET [InvitedAt] = @Now WHERE [Id] = @LouayUserId; -- Louay is invited
UPDATE [dbo].[Users] SET [IsActive] = 0 WHERE [Id] = @SarahUserId; -- Sarah is inactive
