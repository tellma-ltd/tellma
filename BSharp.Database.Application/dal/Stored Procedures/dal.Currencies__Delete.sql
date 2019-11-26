CREATE PROCEDURE [dal].[Currencies__Delete]
	@Ids [dbo].[StringList] READONLY
AS
	DELETE FROM [dbo].[Resources] 
	WHERE [ResourceDefinitionId] = 'monetary-resources' AND [CurrencyId] IN (SELECT Id FROM @Ids);

	DELETE FROM [dbo].[Currencies] 
	WHERE Id IN (SELECT Id FROM @Ids);