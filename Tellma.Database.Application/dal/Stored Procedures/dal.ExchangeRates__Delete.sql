CREATE PROCEDURE [dal].[ExchangeRates__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].ExchangeRates 
	WHERE Id IN (SELECT Id FROM @Ids);