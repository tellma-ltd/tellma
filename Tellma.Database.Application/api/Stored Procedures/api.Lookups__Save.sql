CREATE PROCEDURE [api].[Lookups__Save]
	@DefinitionId INT,
	@Entities [dbo].[LookupList] READONLY,
	@ReturnIds BIT = 0,
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
	EXEC [bll].[Lookups_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@UserId = @UserId,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;
		
	-- (2) Execute
	EXEC [dal].[Lookups__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;