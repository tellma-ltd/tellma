CREATE PROCEDURE [api].[Relations__Save]
	@DefinitionId INT,
	@Entities [dbo].[RelationList] READONLY,
	@RelationUsers [dbo].[RelationUserList] READONLY,
	@Attachments [dbo].[RelationAttachmentList] READONLY,
	@ReturnIds BIT = 0,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT,
	@Culture NVARCHAR(50) = N'en',
	@NeutralCulture NVARCHAR(50) = N'en'
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;
	
	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[Relations_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@RelationUsers = @RelationUsers,
		@Attachments = @Attachments,
		@UserId = @UserId,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	EXEC [dal].[Relations__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@RelationUsers = @RelationUsers,
		@Attachments = @Attachments,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END
