CREATE PROCEDURE [dal].[ResourceDefinitions__Delete]
	@Ids [dbo].[StringList] READONLY
AS
	DELETE [dbo].[ResourceDefinitions] WHERE [Id] IN (SELECT [Id] FROM @Ids);
