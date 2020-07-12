CREATE PROCEDURE [dal].[Lookups__Delete]
	@DefinitionId INT,
	@Ids [dbo].[IdList] READONLY
AS
	DELETE [dbo].[Lookups] WHERE [Id] IN (SELECT [Id] FROM @Ids) AND [DefinitionId] = @DefinitionId;
