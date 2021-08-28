CREATE PROCEDURE [api].[Agents__Save]
	@DefinitionId INT,
	@Entities [dbo].[AgentList] READONLY,
	@AgentUsers [dbo].[AgentUserList] READONLY,
	@Attachments [dbo].[AgentAttachmentList] READONLY,
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
	EXEC [bll].[Agents_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@AgentUsers = @AgentUsers,
		@Attachments = @Attachments,
		@UserId = @UserId,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	EXEC [dal].[Agents__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@AgentUsers = @AgentUsers,
		@Attachments = @Attachments,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END
