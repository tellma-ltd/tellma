CREATE PROCEDURE [dal].[Users__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].[Users] 
	OUTPUT DELETED.[Email]
	WHERE Id IN (SELECT Id FROM @Ids);
