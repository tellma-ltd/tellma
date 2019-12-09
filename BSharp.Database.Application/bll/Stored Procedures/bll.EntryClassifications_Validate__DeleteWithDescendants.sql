CREATE PROCEDURE [bll].[EntryClassifications_Validate__DeleteWithDescendants]
	@Ids [IndexedIdList] READONLY,
	@Top INT = 10
AS
	EXEC [bll].[EntryClassifications_Validate__Delete] @Ids = @Ids, @Top = @Top;