CREATE PROCEDURE [dal].[Documents__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].[Documents] 
	WHERE Id IN (SELECT Id FROM @Ids);