CREATE PROCEDURE [dal].[AdminUsers__SetExternalIdByUserId]
	@UserId INT,
	@ExternalId NVARCHAR(450)
AS
UPDATE [dbo].[AdminUsers] SET [ExternalId] = @ExternalId WHERE [Id] = @UserId
