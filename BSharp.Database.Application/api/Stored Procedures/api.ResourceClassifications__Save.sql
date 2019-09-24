CREATE PROCEDURE [api].[ResourceClassifications__Save]
	@DefinitionId NVARCHAR(50),
	@Entities [ResourceClassificationList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [bll].[ResourceClassifications_Validate__Save]
		@Entities = @Entities;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[ResourceClassifications__Save]
		@DefinitionId= @DefinitionId,
		@Entities = @Entities;
END;