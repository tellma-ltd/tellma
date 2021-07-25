CREATE PROCEDURE [api].[ResourceDefinitions__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT,
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	
	-- Set the global values of the session context
	DECLARE @UserLanguageIndex TINYINT = [dbo].[fn_User__Language](@Culture, @NeutralCulture);
    EXEC sys.sp_set_session_context @key = N'UserLanguageIndex', @value = @UserLanguageIndex;

	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[ResourceDefinitions_Validate__Delete] 
		@Ids = @Ids,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Delete the entities
	EXEC [dal].[ResourceDefinitions__Delete]
		@Ids = @Ids;
END