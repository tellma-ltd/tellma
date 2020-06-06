CREATE PROCEDURE [dal].[Agents__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].[Units] 
	WHERE Id IN (SELECT Id FROM @Ids);