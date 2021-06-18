CREATE PROCEDURE [dal].[Accounts__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Accounts]
	WHERE Id IN (SELECT Id FROM @Ids);
END;
