CREATE PROCEDURE [api].[AccountClassifications__Deprecate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsDeprecated BIT,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	
	INSERT INTO @ValidationErrors
	EXEC [bll].[AccountClassifications_Validate__Deprecate]
		@Ids = @Ids;
	
	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[AccountClassifications__Deprecate]
		@Ids = @Ids,
		@IsDeprecated = @IsDeprecated;
END;