CREATE PROCEDURE [dal].[Documents__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	-- Get a hold of all attachment Ids of the file that are about to be deleted
	DECLARE @DeletedFileIds [dbo].[StringList];
	INSERT INTO @DeletedFileIds ([Id])
	SELECT [FileId] FROM [dbo].[Attachments] WHERE [DocumentId] IN (SELECT [Id] FROM @Ids)

	-- Delete the documents
	DELETE FROM [dbo].[Documents] 
	WHERE [Id] IN (SELECT Id FROM @Ids);

	-- Return deleted attachment Ids
	SELECT [Id] FROM @DeletedFileIds