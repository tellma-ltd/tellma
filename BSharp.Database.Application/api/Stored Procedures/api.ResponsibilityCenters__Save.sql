CREATE PROCEDURE [api].[ResponsibilityCenters__Save]
	@Entities [ResponsibilityCenterList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [dbo].[bll_ResponsibilityCenters_Validate__Save]
		@Entities = @Entities;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[ResponsibilityCenters__Save]
		@Entities = @Entities;
END;