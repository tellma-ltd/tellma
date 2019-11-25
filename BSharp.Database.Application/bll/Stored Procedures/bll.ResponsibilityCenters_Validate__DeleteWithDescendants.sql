CREATE PROCEDURE [bll].[ResponsibilityCenters_Validate__DeleteWithDescendants]
	@Ids [IndexedIdList] READONLY,
	@Top INT = 10
AS
	EXEC [bll].[ResponsibilityCenters_Validate__Delete] @Ids = @Ids, @Top = @Top;