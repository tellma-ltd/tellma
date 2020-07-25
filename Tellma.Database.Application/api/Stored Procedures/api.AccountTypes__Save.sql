CREATE PROCEDURE [api].[AccountTypes__Save]
	@Entities [AccountTypeList] READONLY,
	@AccountTypeResourceDefinitions AccountTypeResourceDefinitionList READONLY,
	@AccountTypeCustodianDefinitions [AccountTypeCustodianDefinitionList] READONLY,
	@AccountTypeNotedRelationDefinitions [AccountTypeNotedRelationDefinitionList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;

	-- Add here Code that is handled by C#
	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[AccountTypes_Validate__Save]
		@Entities = @Entities,
		@AccountTypeResourceDefinitions = @AccountTypeResourceDefinitions,
		@AccountTypeCustodianDefinitions = @AccountTypeCustodianDefinitions,
		@AccountTypeNotedRelationDefinitions = @AccountTypeNotedRelationDefinitions;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[AccountTypes__Save]
		@Entities = @Entities,
		@AccountTypeResourceDefinitions = @AccountTypeResourceDefinitions,
		@AccountTypeCustodianDefinitions = @AccountTypeCustodianDefinitions,
		@AccountTypeNotedRelationDefinitions = @AccountTypeNotedRelationDefinitions;
END;