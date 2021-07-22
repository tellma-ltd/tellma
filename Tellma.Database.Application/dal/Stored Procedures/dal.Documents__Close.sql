CREATE PROCEDURE [dal].[Documents__Close]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	EXEC [dal].[Documents_State__Update]
		@DefinitionId = @DefinitionId,
		@Ids = @Ids,
		@State = 1,
		@UserId = @UserId;
		
	-- This automatically returns the new notification counts
	EXEC [dal].[Documents__Assign]
		@Ids = @Ids,
		@AssigneeId = NULL;
END;
