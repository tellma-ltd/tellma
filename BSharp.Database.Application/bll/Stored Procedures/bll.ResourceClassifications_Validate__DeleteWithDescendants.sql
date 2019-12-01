CREATE PROCEDURE [bll].[ResourceClassifications_Validate__DeleteWithDescendants]
	@Ids [IndexedIdList] READONLY,
	@Top INT = 10
AS
	EXEC [bll].[ResourceClassifications_Validate__Delete] @Ids = @Ids, @Top = @Top;