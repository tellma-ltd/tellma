CREATE PROCEDURE [dal].[GlobalUsers__SetExternalIdByEmail]
	@Email NVARCHAR(255),
	@ExternalId NVARCHAR(450)
AS
UPDATE [dbo].[GlobalUsers] SET [ExternalId] = @ExternalId WHERE [Email] = @Email
