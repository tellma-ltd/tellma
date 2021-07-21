CREATE PROCEDURE [api].[Documents__Assign] 
	@Ids [dbo].[IndexedIdList] READONLY,
	@AssigneeId INT,
	@Comment NVARCHAR(1024),
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
	EXEC [bll].[Documents_Validate__Assign]
		@Ids = @Ids,
		@AssigneeId = @AssigneeId,
		@Comment = @Comment,
		@UserId = @UserId,
		@IsError = @IsError OUTPUT;		

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Execute
	EXEC [dal].[Documents__Assign]
		@Ids = @Ids, 
		@AssigneeId = @AssigneeId,
		@Comment = @Comment,
		@ManualAssignment = 1,
		@UserId = @UserId;
END;