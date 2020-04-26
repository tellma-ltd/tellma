CREATE PROCEDURE [dal].[LookupDefinitions__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE [dbo].[LookupDefinitions] WHERE [Id] IN (SELECT [Id] FROM @Ids);
