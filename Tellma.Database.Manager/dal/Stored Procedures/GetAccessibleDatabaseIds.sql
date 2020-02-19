CREATE PROCEDURE [dal].[GetAccessibleDatabaseIds]
	@ExternalId NVARCHAR(450),
	@Email NVARCHAR(255)
AS
	-- The list of companies the user has access to
	SELECT [M].[DatabaseId] FROM [dbo].[DirectoryUserMemberships] As [M]
	JOIN [dbo].[DirectoryUsers] As [U] ON [M].UserId = [U].[Id]
	WHERE [U].[ExternalId] = @ExternalId OR [U].[Email] = @Email;

	-- Whether the user has access to the admin console
	SELECT [IsAdmin] FROM [dbo].[DirectoryUsers] As [U]
	WHERE [U].[ExternalId] = @ExternalId OR [U].[Email] = @Email;