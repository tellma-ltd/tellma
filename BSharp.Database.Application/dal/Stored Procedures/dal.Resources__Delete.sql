CREATE PROCEDURE [dal].[Resources__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DECLARE @CurrenciesToDelete StringList;

	DELETE FROM [dbo].Resources 
	WHERE Id IN (SELECT Id FROM @Ids);