CREATE PROCEDURE [bll].[AccountTypes_Validate__DeleteWithDescendants]
	@Ids [IndexedIdList] READONLY,
	@Top INT = 10
AS
	EXEC [bll].[AccountTypes_Validate__Delete] @Ids = @Ids, @Top = @Top;