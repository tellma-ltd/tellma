CREATE PROCEDURE [dal].[AgentDefinitions__Delete]
	@Ids [dbo].[StringList] READONLY
AS
	DELETE [dbo].[AgentDefinitions] WHERE [Id] IN (SELECT [Id] FROM @Ids);
