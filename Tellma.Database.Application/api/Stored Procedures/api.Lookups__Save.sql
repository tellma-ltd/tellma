CREATE PROCEDURE [api].[Lookups__Save]
	@DefinitionId NVARCHAR (50),
	@Entities [LookupList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [bll].[Lookups_Validate__Save]
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

	EXEC [dal].[Lookups__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities
END;