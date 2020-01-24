CREATE PROCEDURE [dal].[Roles__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].[Roles] 
	WHERE Id IN (SELECT Id FROM @Ids);
