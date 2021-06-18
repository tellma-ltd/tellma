CREATE PROCEDURE [api].[AccountTypes__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[AccountTypes_Validate__Activate]
		@Ids = @Ids,
		@IsActive = @IsActive,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Activate/Deactivate the entities
	EXEC [dal].[AccountTypes__Activate]
		@Ids = @Ids, 
		@IsActive = @IsActive,
		@UserId = @UserId;
END