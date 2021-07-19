CREATE PROCEDURE [dal].[Users__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Users] 
	OUTPUT DELETED.[Email] -- Returns the deleted emails
	WHERE Id IN (SELECT Id FROM @Ids);
END;
