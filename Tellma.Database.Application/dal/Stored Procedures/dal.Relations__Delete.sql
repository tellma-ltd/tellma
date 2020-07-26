CREATE PROCEDURE [dal].[Relations__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].[Relations] 
	WHERE Id IN (SELECT Id FROM @Ids);