CREATE PROCEDURE [dal].[Users__SetExternalIdByUserId]
	@UserId INT,
	@ExternalId NVARCHAR(450)
AS
UPDATE [dbo].[Users] SET [ExternalId] = @ExternalId WHERE [Id] = @UserId
