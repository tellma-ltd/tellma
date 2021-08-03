CREATE PROCEDURE [dal].[Units__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Units] 
	WHERE Id IN (SELECT Id FROM @Ids);
END;