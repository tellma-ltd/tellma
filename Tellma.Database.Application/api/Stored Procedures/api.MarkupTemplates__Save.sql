CREATE PROCEDURE [api].[MarkupTemplates__Save]
	@Entities [dbo].[MarkupTemplateList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[MarkupTemplates_Validate__Save] 
		@Entities = @Entities,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[MarkupTemplates__Save]
		@Entities = @Entities,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;