CREATE PROCEDURE [api].[AdminUsers__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT
AS
BEGIN
SET NOCOUNT ON;

	-- (1) Validate
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	INSERT INTO @ValidationErrors	
	EXEC [bll].[AdminUsers_Validate__Delete] 
		@Ids = @Ids,
		@UserId = @UserId;

	-- If there are validation errors don't proceed
	IF EXISTS (SELECT * FROM @ValidationErrors)
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[AdminUsers__Delete]
		@Ids = @Ids,
		@UserId = @UserId;
END