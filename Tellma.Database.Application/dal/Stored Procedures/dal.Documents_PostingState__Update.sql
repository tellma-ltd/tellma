CREATE PROCEDURE [dal].[Documents_PostingState__Update]
	@Ids dbo.IdList READONLY,
	@PostingState SMALLINT
AS
UPDATE dbo.Documents
SET
	[PostingState]	= @PostingState,
	[PostingStateAt] = SYSDATETIMEOFFSET(),
	[PostingDate] = IIF(@PostingState = 1,
						COALESCE([PostingDate], CAST(GETDATE() AS DATE)),
						[PostingDate])
WHERE Id IN (SELECT [Id] FROM @Ids)
AND [PostingState] <> @PostingState;