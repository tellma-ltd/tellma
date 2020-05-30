CREATE PROCEDURE [api].[ResourceDefinitions__Save]
	@Entities [ResourceDefinitionList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;

	EXEC [bll].[ResourceDefinitions_Validate__Save]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[ResourceDefinitions__Save]
		@Entities = @Entities,
		@ReturnIds = @ReturnIds;
END