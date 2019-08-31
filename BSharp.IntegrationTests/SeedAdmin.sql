-- TODO: Remove after implementing the provisioning SP
-- This file is executed before all the tests are run

DECLARE @AdminId INT, @ServerId INT, @DatabaseId INT,
@GlobalUserId INT, @Now DATETIMEOFFSET = SYSDATETIMEOFFSET();

SELECT @AdminId = Id FROM [dbo].[AdminUsers] WHERE [Email] = @Email;
SELECT @ServerId = Id FROM [dbo].[SqlServers] WHERE [ServerName] = N'<AdminServer>';

IF NOT EXISTS (SELECT * FROM [dbo].[SqlDatabases])
	INSERT INTO [dbo].[SqlDatabases] ([DatabaseName], [ServerId], [Description], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
	VALUES (@DatabaseName, @ServerId, NULL, @Now, @AdminId, @Now, @AdminId)

IF NOT EXISTS (SELECT * FROM [dbo].[GlobalUsers])
	INSERT INTO [dbo].[GlobalUsers] ([Email])
	VALUES (@Email)
	
IF NOT EXISTS (SELECT * FROM [dbo].[GlobalUserMemberships])
BEGIN
	SELECT @GlobalUserId = Id FROM [dbo].[GlobalUsers] WHERE [Email] = @Email;
	SELECT @DatabaseId = Id FROM [dbo].[SqlDatabases] WHERE [DatabaseName] = @DatabaseName;

	INSERT INTO [dbo].[GlobalUserMemberships] ([UserId], [DatabaseId])
	VALUES (@GlobalUserId, @DatabaseId);
END;
