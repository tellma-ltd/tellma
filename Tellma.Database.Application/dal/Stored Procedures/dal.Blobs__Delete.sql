CREATE PROCEDURE [dal].[Blobs__Delete]
	@BlobNames [dbo].[StringList] READONLY
AS
DELETE FROM [dbo].[Blobs] WHERE [Id] IN (SELECT [Id] FROM @BlobNames)