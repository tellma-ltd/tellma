CREATE PROCEDURE [api].[AgentRelations__Save]
	@DefinitionId NVARCHAR(255),
	@Entities [AgentRelationList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [bll].[AgentRelations_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[AgentRelations__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@ReturnIds = @ReturnIds;
END