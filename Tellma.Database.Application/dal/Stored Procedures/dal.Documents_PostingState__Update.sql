CREATE PROCEDURE [dal].[Documents_PostingState__Update]
	@Ids dbo.IdList READONLY,
	@PostingState SMALLINT
AS
UPDATE dbo.Documents
SET
	[PostingState]	= @PostingState,
	[PostingStateAt] = SYSDATETIMEOFFSET()
WHERE Id IN (SELECT [Id] FROM @Ids)
AND [PostingState] <> @PostingState;