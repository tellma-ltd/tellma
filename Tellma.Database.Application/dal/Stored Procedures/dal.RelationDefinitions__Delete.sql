CREATE PROCEDURE [dal].[RelationDefinitions__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE [dbo].[RelationDefinitions] WHERE [Id] IN (SELECT [Id] FROM @Ids);
