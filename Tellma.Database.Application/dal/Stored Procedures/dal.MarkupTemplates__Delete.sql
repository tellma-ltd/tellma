CREATE PROCEDURE [dal].[MarkupTemplates__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].[MarkupTemplates] 
	WHERE Id IN (SELECT Id FROM @Ids);
