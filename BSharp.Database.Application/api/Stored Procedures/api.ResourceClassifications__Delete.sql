CREATE PROCEDURE [api].[ResourceClassifications__Delete]
	@IndexedIds [IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [bll].[ResourceClassifications_Validate__Delete]
		@Ids = @IndexedIds;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	DECLARE @Ids dbo.IdList;
	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[ResourceClassifications__Delete]
		@Ids = @Ids;
END;