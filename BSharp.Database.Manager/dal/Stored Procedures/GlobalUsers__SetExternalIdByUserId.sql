CREATE PROCEDURE [dal].[GlobalUsers__SetExternalIdByUserId]
	@UserId INT,
	@ExternalId NVARCHAR(450)
AS
UPDATE [dbo].[GlobalUsers] SET [ExternalId] = @ExternalId WHERE [Id] = @UserId
