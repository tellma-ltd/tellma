CREATE PROCEDURE [api].[IfrsDisclosureDetails__Save]
	@Entities [IfrsDisclosureDetailList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[IfrsDisclosureDetails_Validate__Save]
		@Entities = @Entities;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);


	IF @ValidationErrorsJson IS NOT NULL
		RETURN;
	
	EXEC [dal].[IfrsDisclosureDetails__Save]
		@Entities = @Entities;
END;