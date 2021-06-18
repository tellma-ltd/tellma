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

	-- (1) Preprocess the entities
	-- TODO
	DECLARE @Preprocessed [dbo].[AccountTypeList];
	INSERT INTO @Preprocessed
	SELECT * FROM @Entities;	

	-- (2) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[AccountTypes_Validate__Save] 
		@Entities = @Preprocessed,
		@AccountTypeRelationDefinitions = @AccountTypeRelationDefinitions,
		@AccountTypeResourceDefinitions = @AccountTypeResourceDefinitions,
		@AccountTypeNotedRelationDefinitions = @AccountTypeNotedRelationDefinitions,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (3) Save the entities
	EXEC [dal].[AccountTypes__Save]
		@Entities = @Preprocessed,
		@AccountTypeRelationDefinitions = @AccountTypeRelationDefinitions,
		@AccountTypeResourceDefinitions = @AccountTypeResourceDefinitions,
		@AccountTypeNotedRelationDefinitions = @AccountTypeNotedRelationDefinitions,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;