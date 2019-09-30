CREATE PROCEDURE [bll].[AccountClassifications_Validate__DeleteWithDescendants]
	@Ids [IndexedIdList] READONLY,
	@Top INT = 10
AS
	-- TODO: this is merely caling [bll].[ProductCategories_Validate__Delete]
	EXEC [bll].[AccountClassifications_Validate__Delete] @Ids = @Ids, @Top = @Top;