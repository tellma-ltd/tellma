CREATE PROCEDURE [dal].[GetDatabaseConnectionInfo]
	@DatabaseId int
AS
SET NOCOUNT ON;

SELECT [S].[ServerName], [D].[DatabaseName], [S].[UserName], [S].[PasswordKey]
FROM [dbo].[SqlServers] AS [S] JOIN [dbo].[SqlDatabases] AS [D] ON [S].[Id] = [D].[ServerId]
WHERE [D].[Id] = @DatabaseId

