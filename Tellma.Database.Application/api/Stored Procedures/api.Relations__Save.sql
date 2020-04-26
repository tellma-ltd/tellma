CREATE PROCEDURE [api].[Relations__Save]
	@DefinitionId INT,
	@Entities [RelationList] READONLY,
	@AgentRates [AgentRateList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [bll].[Relations_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@AgentRates = @AgentRates;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Relations__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@AgentRates = @AgentRates,
		@ReturnIds = @ReturnIds;
END