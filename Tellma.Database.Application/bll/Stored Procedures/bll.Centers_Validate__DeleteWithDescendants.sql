CREATE PROCEDURE [bll].[Centers_Validate__DeleteWithDescendants]
	@Ids [IndexedIdList] READONLY,
	@Top INT = 10
AS
	EXEC [bll].[Centers_Validate__Delete] @Ids = @Ids, @Top = @Top;