CREATE PROCEDURE [api].[ResourceDefinitions__UpdateState]
	@Ids [dbo].[IndexedIdList] READONLY,
	@State NVARCHAR(50),
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
	EXEC [bll].[ResourceDefinitions_Validate__UpdateState]
		@Ids = @Ids,
		@State = @State,
		@UserId = @UserId,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Activate/Deactivate the entities
	EXEC [dal].[ResourceDefinitions__UpdateState]
		@Ids = @Ids, 
		@State = @State,
		@UserId = @UserId;
END