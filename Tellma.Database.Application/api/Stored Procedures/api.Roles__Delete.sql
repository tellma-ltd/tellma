CREATE PROCEDURE [api].[Roles__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[Roles_Validate__Delete] 
		@Ids = @Ids,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;
		
	-- (2) Execute
	EXEC [dal].[Roles__Delete]
		@Ids = @Ids;
END;