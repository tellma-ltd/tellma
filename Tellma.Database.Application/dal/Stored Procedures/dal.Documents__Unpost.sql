CREATE PROCEDURE [dal].[Documents__Unpost]
	@Ids [dbo].[IdList] READONLY
AS
BEGIN
	EXEC [dal].[Documents_PostingState__Update]
		@Ids = @Ids,
		@PostingState = 0;

	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = @UserId;
END;