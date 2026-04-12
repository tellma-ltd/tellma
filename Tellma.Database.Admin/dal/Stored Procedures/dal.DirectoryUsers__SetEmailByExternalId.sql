CREATE PROCEDURE [dal].[DirectoryUsers__SetEmailByExternalId]
	@ExternalId NVARCHAR(450),
	@Email NVARCHAR(255)
AS
UPDATE [dbo].[DirectoryUsers] SET [EmailOrClientId] = @Email WHERE [ExternalId] = @ExternalId
