CREATE PROCEDURE [api].[AgentDefinitions__Save]
	@Entities [AgentDefinitionList] READONLY,
	--@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [bll].[AgentDefinitions_Validate__Save]
		@Entities = @Entities;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[AgentDefinitions__Save]
		@Entities = @Entities;
		--@ReturnIds = @ReturnIds;
END