CREATE PROCEDURE [dal].[Documents__Uncancel]
	@Ids [dbo].[IdList] READONLY
AS
BEGIN
	EXEC [dal].[Documents_State__Update]
		@Ids = @Ids,
		@State = 0;
		
	-- This automatically returns the new notification counts
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = @UserId;
END;