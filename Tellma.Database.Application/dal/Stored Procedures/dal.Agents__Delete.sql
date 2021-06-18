CREATE PROCEDURE [dal].[Agents__Delete]
	@Ids [dbo].[IdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Agents] 
	WHERE Id IN (SELECT Id FROM @Ids);
END;