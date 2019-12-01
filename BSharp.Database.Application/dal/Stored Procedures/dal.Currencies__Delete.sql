CREATE PROCEDURE [dal].[Currencies__Delete]
	@Ids [dbo].[StringList] READONLY
AS
	DELETE FROM [dbo].[Resources] 
	WHERE [DefinitionId] = 'monetary-resources'
	AND ResourceClassificationId = dbo.fn_RCCOde__Id(N'Cash')
	AND [CurrencyId] IN (SELECT Id FROM @Ids);

	DELETE FROM [dbo].[Currencies] 
	WHERE Id IN (SELECT Id FROM @Ids);