CREATE PROCEDURE [api].[LineDefinitions__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[LineDefinitions_Validate__Delete] 
		@Ids = @Ids,
		@UserId = @UserId,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Delete the entities
	EXEC [dal].[LineDefinitions__Delete]
		@Ids = @Ids;
END