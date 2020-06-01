CREATE PROCEDURE [api].[LookupDefinitions__Save]
	@Entities [LookupDefinitionList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors ValidationErrorList;
	-- Id must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT 
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Id',
		N'Error_TheCode0IsDuplicated',
		[Code]
	FROM @Entities
	WHERE [Code] IN (
		SELECT [Code] FROM @Entities
		GROUP BY [Code]
		HAVING COUNT(*) > 1
	);

	INSERT INTO @ValidationErrors
	EXEC [bll].[LookupDefinitions_Validate__Save]
		@Entities = @Entities;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[LookupDefinitions__Save]
		@Entities = @Entities,
		@ReturnIds = @ReturnIds;
END