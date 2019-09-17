CREATE PROCEDURE [dal].[ResourceLookups__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE [dbo].[ResourceLookups] WHERE [Id] IN (SELECT [Id] FROM @Ids);
