CREATE PROCEDURE [dal].[Resources__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].Resources 
	WHERE Id IN (SELECT Id FROM @Ids);