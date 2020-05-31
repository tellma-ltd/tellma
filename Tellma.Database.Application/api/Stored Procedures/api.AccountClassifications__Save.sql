CREATE PROCEDURE [api].[AccountClassifications__Save]
	@Entities [AccountClassificationList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	-- Add here Code that is handled by C#

	EXEC [bll].[AccountClassifications_Validate__Save]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[AccountClassifications__Save]
		@Entities = @Entities;
END