CREATE PROCEDURE [dal].[Accounts__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].[Accounts]
	WHERE Id IN (SELECT Id FROM @Ids);
