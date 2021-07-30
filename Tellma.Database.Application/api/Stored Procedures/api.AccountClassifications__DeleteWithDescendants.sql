CREATE PROCEDURE [api].[AccountClassifications__DeleteWithDescendants]
	@Ids [dbo].[IndexedIdList] READONLY,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
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
	EXEC [bll].[AccountClassifications_Validate__DeleteWithDescendants] 
		@Ids = @Ids,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Delete the entities
	EXEC [dal].[AccountClassifications__DeleteWithDescendants]
		@Ids = @Ids;
END