CREATE PROCEDURE [dal].[Documents__Close]
	@Ids [dbo].[IdList] READONLY
AS
BEGIN
	EXEC [dal].[Documents_State__Update]
		@Ids = @Ids,
		@State = 1;
		
	-- This automatically returns the new notification counts
	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = NULL;
END;
