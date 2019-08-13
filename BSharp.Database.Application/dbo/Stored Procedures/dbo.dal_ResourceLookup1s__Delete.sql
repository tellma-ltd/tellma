CREATE PROCEDURE [dbo].[dal_ResourceLookup1s__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE [dbo].ResourceLookup1s WHERE [Id] IN (SELECT [Id] FROM @Ids);