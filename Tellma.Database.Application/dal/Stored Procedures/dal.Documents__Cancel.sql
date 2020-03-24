CREATE PROCEDURE [dal].[Documents__Cancel]
	@Ids [dbo].[IdList] READONLY
AS
BEGIN
	EXEC [dal].[Documents_State__Update]
		@Ids = @Ids,
		@State = -1;

	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = NULL;
END;