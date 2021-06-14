CREATE PROCEDURE [api].[AccountClassifications__Delete]
	@Entities [AccountClassificationList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	-- Add here Code that is handled by C#

	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[AccountClassifications_Validate__Save]
		@Entities = @Entities;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);
	
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[AccountClassifications__Save]
		@Entities = @Entities;
END