CREATE PROCEDURE [dal].[Contracts__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].[Contracts] 
	WHERE Id IN (SELECT Id FROM @Ids);