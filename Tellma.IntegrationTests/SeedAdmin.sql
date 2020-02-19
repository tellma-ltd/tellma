-- This file is executed before any test is run

DECLARE @AdminId INT, @ServerId INT, @DatabaseId INT,
@DirectoryUserId INT, @Now DATETIMEOFFSET = SYSDATETIMEOFFSET();

SELECT @AdminId = Id FROM [dbo].[AdminUsers] WHERE [Email] = @Email;
SELECT @ServerId = Id FROM [dbo].[SqlServers] WHERE [ServerName] = N'<AdminServer>';

IF NOT EXISTS (SELECT * FROM [dbo].[SqlDatabases] WHERE [DatabaseName] = @DatabaseName)
	INSERT INTO [dbo].[SqlDatabases] ([DatabaseName], [ServerId], [Description], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
	VALUES (@DatabaseName, @ServerId, NULL, @Now, @AdminId, @Now, @AdminId)

IF NOT EXISTS (SELECT * FROM [dbo].[DirectoryUsers])
	INSERT INTO [dbo].[DirectoryUsers] ([Email])
	VALUES (@Email)
	
IF NOT EXISTS (SELECT * FROM [dbo].[DirectoryUserMemberships])
BEGIN
	SELECT @DirectoryUserId = Id FROM [dbo].[DirectoryUsers] WHERE [Email] = @Email;
	SELECT @DatabaseId = Id FROM [dbo].[SqlDatabases] WHERE [DatabaseName] = @DatabaseName;

	INSERT INTO [dbo].[DirectoryUserMemberships] ([UserId], [DatabaseId])
	VALUES (@DirectoryUserId, @DatabaseId);
END;
