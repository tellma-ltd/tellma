CREATE PROCEDURE [api].[MarkupTemplates__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[MarkupTemplates_Validate__Delete] 
		@Ids = @Ids,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;
		
	-- (2) Execute
	EXEC [dal].[MarkupTemplates__Delete]
		@Ids = @Ids;
END