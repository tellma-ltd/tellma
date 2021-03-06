﻿CREATE PROCEDURE [dal].[AdminUsers__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	-- Sync with Directory Users
	UPDATE [dbo].[DirectoryUsers] SET [IsAdmin] = 0
	WHERE [Email] IN (SELECT [Email] FROM [dbo].[AdminUsers] WHERE Id IN (SELECT Id FROM @Ids))

	-- Delete
	DELETE FROM [dbo].[AdminUsers] 
	WHERE Id IN (SELECT Id FROM @Ids);
