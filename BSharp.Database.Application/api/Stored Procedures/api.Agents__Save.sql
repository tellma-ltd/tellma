CREATE PROCEDURE [api].[Agents__Save]
	@DefinitionId NVARCHAR(50),
	@Entities [AgentList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [bll].[Agents_Validate__Save]
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

	EXEC [dal].[Agents__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@ReturnIds = @ReturnIds;
END