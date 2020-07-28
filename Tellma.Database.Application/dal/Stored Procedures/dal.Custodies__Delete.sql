CREATE PROCEDURE [dal].[Custodies__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].[Custodies] 
	WHERE Id IN (SELECT Id FROM @Ids);