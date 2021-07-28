CREATE PROCEDURE [api].[AdminUsers__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@UserId INT
AS
BEGIN
SET NOCOUNT ON;

	-- (1) Validate
	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors	
	EXEC [bll].[AdminUsers_Validate__Activate] 
		@Ids = @Ids,
		@IsActive = @IsActive,
		@UserId = @UserId;

	-- (2) Return the Validation Errors (if any)
	SELECT * FROM @ValidationErrors;

	-- (3) If there are validation errors don't proceed
	IF EXISTS (SELECT * FROM @ValidationErrors)
		RETURN;

	-- (4) Save the entities
	EXEC [dal].[AdminUsers__Activate]
		@Ids = @Ids,
		@IsActive = @IsActive,
		@UserId = @UserId;	

END
