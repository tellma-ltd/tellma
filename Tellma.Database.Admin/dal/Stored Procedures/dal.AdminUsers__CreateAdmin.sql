CREATE PROCEDURE [dal].[AdminUsers__CreateAdmin]
	@Email NVARCHAR(255),
	@FullName NVARCHAR(255),
	@AdminServerDescription NVARCHAR(1024) = NULL
AS
DECLARE @Now DATETIMEOFFSET = SYSDATETIMEOFFSET();
IF NOT EXISTS (SELECT * FROM [dbo].[AdminUsers] WHERE [Email] = @Email)
BEGIN
	INSERT INTO [dbo].[AdminUsers] ([Name], [Email], [CreatedById], [CreatedAt], [ModifiedById], [ModifiedAt])
	VALUES (@FullName, @Email, IDENT_CURRENT('dbo.AdminUsers'), @Now, IDENT_CURRENT('dbo.AdminUsers'), @Now);
END

IF NOT EXISTS (SELECT * FROM [dbo].[DirectoryUsers] WHERE [Email] = @Email)
BEGIN
	INSERT INTO [dbo].[DirectoryUsers] ([Email], [IsAdmin])
	VALUES (@Email, 1);
END

DECLARE @AdminId INT;
SELECT @AdminId = Id FROM [dbo].[AdminUsers] WHERE [Email] = @Email;

IF NOT EXISTS (SELECT * FROM [dbo].[AdminPermissions] WHERE [AdminUserId] = @AdminId)
BEGIN
	INSERT INTO [dbo].[AdminPermissions] ([AdminUserId], [View], [Action], [Criteria], [Memo], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
	VALUES (@AdminId, N'all', N'All', NULL, NULL, @Now, @AdminId, @Now, @AdminId)
END

IF NOT EXISTS (SELECT * FROM [dbo].[SqlServers] WHERE [ServerName] = N'<AdminServer>')
BEGIN

	INSERT INTO [dbo].[SqlServers] ([ServerName], [UserName], [PasswordKey], [Description], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
	VALUES (N'<AdminServer>', N'', NULL, @AdminServerDescription, @Now, @AdminId, @Now, @AdminId)
END

IF NOT EXISTS (SELECT * FROM [dbo].[AdminSettings])
BEGIN
	INSERT INTO [dbo].[AdminSettings] ([SettingsVersion], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
	VALUES (NEWID(), @Now, @AdminId, @Now,@AdminId)
END
