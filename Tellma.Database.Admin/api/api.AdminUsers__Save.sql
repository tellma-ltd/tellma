CREATE PROCEDURE [api].[AdminUsers__Save]
	@Entities [dbo].[AdminUserList] READONLY,
	@Permissions [dbo].[AdminPermissionList] READONLY,
	@UserId INT,
	@ReturnIds BIT = 0
AS
BEGIN
SET NOCOUNT ON;

	-- (1) Preprocess the entities
	-- TODO
	DECLARE @Preprocessed [dbo].[AdminUserList];
	INSERT INTO @Preprocessed
	SELECT * FROM @Entities;

	-- (2) Validate the Entities
	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors	
	EXEC [bll].[AdminUsers_Validate__Save] 
		@Entities = @Preprocessed, 
		@Permissions = @Permissions,
		@UserId = @UserId

	-- (3) Return the Validation Errors (if any)
	SELECT * FROM @ValidationErrors;

	-- (4) If there are validation errors don't proceed
	IF EXISTS (SELECT * FROM @ValidationErrors)
		RETURN;

	-- (5) Save the entities
	EXEC [dal].[AdminUsers__Save]
		@Entities = @Preprocessed,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;

END