CREATE PROCEDURE [api].[AccountTypes__DeleteWithDescendants]
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[AccountTypes_Validate__DeleteWithDescendants] 
		@Ids = @Ids,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Delete the entities
	EXEC [dal].[AccountTypes__DeleteWithDescendants]
		@Ids = @Ids;
END