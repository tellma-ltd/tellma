CREATE PROCEDURE [dal].[Documents__Open]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	EXEC [dal].[Documents_State__Update]
		@DefinitionId = @DefinitionId,
		@Ids = @Ids,
		@State = 0,
		@UserId = @UserId;
	
	-- This automatically returns the new notification counts
	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = @UserId,
		@UserId = @UserId;

	-- Delete all reconciliations pertaining to the cash entries in this document, if any.
	DELETE FROM Reconciliations WHERE Id IN	(
		SELECT ReconciliationId
		FROM ReconciliationEntries
		WHERE EntryId IN (
			SELECT Id
			FROM dbo.Entries
			WHERE LineId IN (
				SELECT Id
				FROM dbo.Lines
				WHERE DocumentId IN (SELECT Id FROM @Ids)
			)
		)
	)
END;