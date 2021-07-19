CREATE PROCEDURE [api].[AccountTypes__Save]
	@Entities [AccountTypeList] READONLY,
	@AccountTypeRelationDefinitions AccountTypeRelationDefinitionList READONLY,
	@AccountTypeResourceDefinitions AccountTypeResourceDefinitionList READONLY,
	@AccountTypeNotedRelationDefinitions AccountTypeNotedRelationDefinitionList READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[AccountTypes_Validate__Save] 
		@Entities = @Entities,
		@AccountTypeRelationDefinitions = @AccountTypeRelationDefinitions,
		@AccountTypeResourceDefinitions = @AccountTypeResourceDefinitions,
		@AccountTypeNotedRelationDefinitions = @AccountTypeNotedRelationDefinitions,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
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