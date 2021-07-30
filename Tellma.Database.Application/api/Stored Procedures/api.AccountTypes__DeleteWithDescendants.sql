CREATE PROCEDURE [api].[AccountTypes__DeleteWithDescendants]
	@Ids [dbo].[IndexedIdList] READONLY,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[AccountTypes_Validate__DeleteWithDescendants] 
		@Ids = @Ids,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Delete the entities
	EXEC [dal].[AccountTypes__DeleteWithDescendants]
		@Ids = @Ids;
END