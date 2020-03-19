CREATE PROCEDURE [dal].[Documents__Post]
	@Ids [dbo].[IdList] READONLY
AS
BEGIN
	EXEC [dal].[Documents_PostingState__Update]
		@Ids = @Ids,
		@PostingState = 1;

	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = NULL;
END;
