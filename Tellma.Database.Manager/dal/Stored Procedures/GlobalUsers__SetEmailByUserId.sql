CREATE PROCEDURE [dal].[GlobalUsers__SetEmailByUserId]
	@UserId INT,
	@ExternalEmail NVARCHAR(255)
AS
UPDATE [dbo].[GlobalUsers] SET [Email] = @ExternalEmail WHERE [Id] = @UserId
