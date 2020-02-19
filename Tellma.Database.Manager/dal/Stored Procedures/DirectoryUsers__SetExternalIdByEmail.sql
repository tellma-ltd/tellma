CREATE PROCEDURE [dal].[DirectoryUsers__SetExternalIdByEmail]
	@Email NVARCHAR(255),
	@ExternalId NVARCHAR(450)
AS
UPDATE [dbo].[DirectoryUsers] SET [ExternalId] = @ExternalId WHERE [Email] = @Email
