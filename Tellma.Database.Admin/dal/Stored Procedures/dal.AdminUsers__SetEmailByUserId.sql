CREATE PROCEDURE [dal].[AdminUsers__SetEmailByUserId]
	@UserId INT,
	@ExternalEmail NVARCHAR(255)
AS
UPDATE [dbo].[AdminUsers] SET [Email] = @ExternalEmail WHERE [Id] = @UserId
