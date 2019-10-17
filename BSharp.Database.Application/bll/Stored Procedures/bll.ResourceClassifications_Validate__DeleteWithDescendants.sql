CREATE PROCEDURE [bll].[ResourceClassifications_Validate__DeleteWithDescendants]
	@DefinitionId NVARCHAR(50),
	@Ids [IndexedIdList] READONLY,
	@Top INT = 10
AS
	EXEC [bll].[ResourceClassifications_Validate__Delete] @DefinitionId = @DefinitionId, @Ids = @Ids, @Top = @Top;