CREATE PROCEDURE [api].[AccountTypes__DeleteWithDescendants]
	@IndexedIds [IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	-- Add here Code that is handled by C#
	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[AccountTypes_Validate__DeleteWithDescendants]
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
	EXEC [dal].[AccountTypes__DeleteWithDescendants]
		@Ids = @Ids;
END;