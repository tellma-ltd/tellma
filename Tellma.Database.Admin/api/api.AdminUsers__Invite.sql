CREATE PROCEDURE [api].[AdminUsers__Invite]
	@Ids [dbo].[IndexedIdList] READONLY,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[AdminUsers_Validate__Invite] 
		@Ids = @Ids,
		@Top = @Top,
		@UserId = @UserId,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (4) Save the entities
	EXEC [dal].[AdminUsers__Invite]
		@Ids = @Ids,
		@UserId = @UserId;
END
