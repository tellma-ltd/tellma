CREATE PROCEDURE [dal].[Blobs__Get]
	@Name NVARCHAR(450)
AS
	SELECT [Content] FROM [dbo].[Blobs] WHERE [Id] = @Name;
