CREATE PROCEDURE [dal].[Documents__Cancel]
	@Ids [dbo].[IdList] READONLY
AS
BEGIN
	EXEC [dal].[Documents_PostingState__Update]
		@Ids = @Ids,
		@PostingState = -1;

	-- TODO: Update all lines that don't have a workflow to VOID and refresh document state

	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = NULL;
END;