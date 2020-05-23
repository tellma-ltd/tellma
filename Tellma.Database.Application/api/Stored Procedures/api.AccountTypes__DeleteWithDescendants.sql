CREATE PROCEDURE [api].[AccountTypes__DeleteWithDescendants]
	@IndexedIds [IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	-- Add here Code that is handled by C#

	EXEC [bll].[AccountTypes_Validate__DeleteWithDescendants]
		@Ids = @IndexedIds,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	DECLARE @Ids dbo.IdList;
	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[AccountTypes__DeleteWithDescendants]
		@Ids = @Ids;
END;