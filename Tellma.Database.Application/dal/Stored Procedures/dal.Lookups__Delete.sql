CREATE PROCEDURE [dal].[Lookups__Delete]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Lookups] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids) AND [DefinitionId] = @DefinitionId;
END;