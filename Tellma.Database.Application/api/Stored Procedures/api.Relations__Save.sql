CREATE PROCEDURE [api].[Relations__Save]
	@DefinitionId INT,
	@Entities [dbo].[RelationList] READONLY,
	@RelationUsers [dbo].[RelationUserList] READONLY,
	@Attachments [dbo].[RelationAttachmentList] READONLY,
	@ReturnIds BIT = 0,
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
	EXEC [bll].[Relations_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@RelationUsers = @RelationUsers,
		@Attachments = @Attachments,
		@UserId = @UserId,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	EXEC [dal].[Relations__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@RelationUsers = @RelationUsers,
		@Attachments = @Attachments,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END
