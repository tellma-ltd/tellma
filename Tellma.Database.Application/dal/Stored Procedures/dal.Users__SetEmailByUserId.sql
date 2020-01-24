CREATE PROCEDURE [dal].[Users__SetEmailByUserId]
	@UserId INT,
	@ExternalEmail NVARCHAR(255)
AS
UPDATE [dbo].[Users] SET [Email] = @ExternalEmail WHERE [Id] = @UserId