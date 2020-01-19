CREATE PROCEDURE [bll].[EntryTypes_Validate__DeleteWithDescendants]
	@Ids [IndexedIdList] READONLY,
	@Top INT = 10
AS
	EXEC [bll].[EntryTypes_Validate__Delete] @Ids = @Ids, @Top = @Top;