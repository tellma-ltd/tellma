﻿CREATE PROCEDURE [dal].[Documents__Delete]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	-- Get a hold of all attachment Ids of the file that are about to be deleted
	DECLARE @DeletedFileIds [dbo].[StringList];
	INSERT INTO @DeletedFileIds ([Id])
	SELECT [FileId] FROM [dbo].[Attachments] WHERE [DocumentId] IN (SELECT [Id] FROM @Ids);
	
	-- Calculate notification infos for all affected users
	DECLARE @AffectedUsers [dbo].[IdList];
	INSERT INTO @AffectedUsers
	SELECT DISTINCT [AssigneeId]
	FROM [dbo].[DocumentAssignments]
	WHERE [DocumentId] IN (SELECT [Id] FROM @Ids);

	-- Delete the documents
	DELETE FROM [dbo].[Documents] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids) AND [DefinitionId] = @DefinitionId; -- You can only delete from one definition at a time
	
	-- Return Notification info
	EXEC [dal].[InboxCounts__Load] @UserIds = @AffectedUsers;

	-- Return deleted attachment Ids
	SELECT [Id] FROM @DeletedFileIds;
END;