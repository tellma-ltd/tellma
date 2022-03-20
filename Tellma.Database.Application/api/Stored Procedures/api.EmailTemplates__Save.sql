CREATE PROCEDURE [api].[EmailTemplates__Save]
	@Entities [dbo].[EmailTemplateList] READONLY,
	@Parameters [dbo].[EmailTemplateParameterList] READONLY,
	@Attachments [dbo].[EmailTemplateAttachmentList] READONLY,
	@Subscribers [dbo].[EmailTemplateSubscriberList] READONLY,
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

	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[EmailTemplates_Validate__Save] 
		@Entities = @Entities,
		@Parameters	= @Parameters,
		@Attachments = @Attachments,
		@Subscribers = @Subscribers,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[EmailTemplates__Save]
		@Entities = @Entities,
		@Parameters	= @Parameters,
		@Attachments = @Attachments,
		@Subscribers = @Subscribers,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;