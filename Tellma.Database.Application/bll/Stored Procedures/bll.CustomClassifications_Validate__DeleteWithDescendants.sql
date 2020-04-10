CREATE PROCEDURE [bll].[CustomClassifications_Validate__DeleteWithDescendants]
	@Ids [IndexedIdList] READONLY,
	@Top INT = 10
AS
	EXEC [bll].[CustomClassifications_Validate__Delete] @Ids = @Ids, @Top = @Top;