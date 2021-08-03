CREATE PROCEDURE [dal].[ExchangeRates__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[ExchangeRates] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
END;