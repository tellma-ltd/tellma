CREATE PROCEDURE [dal].[Lookups__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE [dbo].[Lookups] WHERE [Id] IN (SELECT [Id] FROM @Ids);
