CREATE PROCEDURE [dal].[Currencies__Delete]
	@Ids [dbo].[StringList] READONLY
AS
	DELETE FROM [dbo].[Currencies] 
	WHERE Id IN (SELECT Id FROM @Ids);