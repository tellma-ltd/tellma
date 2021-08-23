CREATE PROCEDURE [api].[AccountTypes__Save]
	@Entities [AccountTypeList] READONLY,
	@AccountTypeRelationDefinitions AccountTypeRelationDefinitionList READONLY,
	@AccountTypeResourceDefinitions AccountTypeResourceDefinitionList READONLY,
	@AccountTypeNotedRelationDefinitions AccountTypeNotedRelationDefinitionList READONLY,
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
	EXEC [bll].[AccountTypes_Validate__Save] 
		@Entities = @Entities,
		@AccountTypeRelationDefinitions = @AccountTypeRelationDefinitions,
		@AccountTypeResourceDefinitions = @AccountTypeResourceDefinitions,
		@AccountTypeNotedRelationDefinitions = @AccountTypeNotedRelationDefinitions,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[AccountTypes__Save]
		@Entities = @Entities,
		@AccountTypeRelationDefinitions = @AccountTypeRelationDefinitions,
		@AccountTypeResourceDefinitions = @AccountTypeResourceDefinitions,
		@AccountTypeNotedRelationDefinitions = @AccountTypeNotedRelationDefinitions,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;