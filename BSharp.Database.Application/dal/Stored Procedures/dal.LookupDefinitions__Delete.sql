CREATE PROCEDURE [dal].[LookupDefinitions__Delete]
	@Ids [dbo].[StringList] READONLY
AS
	DELETE [dbo].[LookupDefinitions] WHERE [Id] IN (SELECT [Id] FROM @Ids);
