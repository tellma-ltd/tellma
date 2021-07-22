CREATE PROCEDURE [api].[EntryTypes__Save]
	@Entities [dbo].[EntryTypeList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[EntryTypes_Validate__Save] 
		@Entities = @Entities,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[EntryTypes__Save]
		@Entities = @Entities,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;