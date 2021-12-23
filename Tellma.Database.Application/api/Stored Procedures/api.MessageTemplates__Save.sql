CREATE PROCEDURE [api].[MessageTemplates__Save]
	@Entities [dbo].[MessageTemplateList] READONLY,
	@Parameters [dbo].[MessageTemplateParameterList] READONLY,
	@Subscribers [dbo].[MessageTemplateSubscriberList] READONLY,
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
	EXEC [bll].[MessageTemplates_Validate__Save] 
		@Entities = @Entities,
		@Parameters	= @Parameters,
		@Subscribers = @Subscribers,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[MessageTemplates__Save]
		@Entities = @Entities,
		@Parameters	= @Parameters,
		@Subscribers = @Subscribers,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;