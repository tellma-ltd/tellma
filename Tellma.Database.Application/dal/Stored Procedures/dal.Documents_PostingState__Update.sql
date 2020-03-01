CREATE PROCEDURE [dal].[Documents_PostingState__Update]
	@Ids dbo.IdList READONLY,
	@PostingState SMALLINT
AS
UPDATE D
SET
	[PostingState]	= @PostingState,
	[PostingStateAt] = SYSDATETIMEOFFSET()
FROM dbo.Documents
WHERE Id IN (SELECT [Id] FROM @Ids)
AND [PostingState] <> @PostingState;