CREATE PROCEDURE [api].[Lookups__Save]
	@DefinitionId INT,
	@Entities [LookupList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;

	EXEC [bll].[Lookups_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Lookups__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities
END;