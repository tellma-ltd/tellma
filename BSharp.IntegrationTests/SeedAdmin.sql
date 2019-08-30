-- TODO: Remove after implementing the provisioning SP
-- This file is executed before all the tests are run

DECLARE @AdminId INT, @Now DATETIMEOFFSET = SYSDATETIMEOFFSET();
SELECT @AdminId = Id FROM [dbo].[AdminUsers] WHERE [Email] = N'admin@bsharp.online';

IF NOT EXISTS (SELECT * FROM [dbo].[SqlDatabases])
	INSERT INTO [dbo].[SqlDatabases] ([DatabaseName], [ServerId], [Description], [CreatedAt], [CreatedById], [ModifiedAt], [ModifiedById])
	VALUES (N'BSharp.101', 2, NULL, @Now, @AdminId, @Now, @AdminId)

IF NOT EXISTS (SELECT * FROM [dbo].[GlobalUsers])
	INSERT INTO [dbo].[GlobalUsers] ([Email]) Values (N'admin@bsharp.online')

IF NOT EXISTS (SELECT * FROM [dbo].[GlobalUserMemberships])
	INSERT INTO [dbo].[GlobalUserMemberships] ([UserId], [DatabaseId]) Values (2, 101)