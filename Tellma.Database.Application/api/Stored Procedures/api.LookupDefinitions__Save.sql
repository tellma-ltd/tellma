CREATE PROCEDURE [api].[LookupDefinitions__Save]
	@Entities [LookupDefinitionList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;

	EXEC [bll].[LookupDefinitions_Validate__Save]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[LookupDefinitions__Save]
		@Entities = @Entities,
		@ReturnIds = @ReturnIds;
END