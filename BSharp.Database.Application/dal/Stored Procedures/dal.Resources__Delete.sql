CREATE PROCEDURE [dal].[Resources__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DECLARE @CurrenciesToDelete StringList;

	INSERT INTO @CurrenciesToDelete
	SELECT DISTINCT CurrencyId FROM dbo.Resources
	WHERE [Id] IN (SELECT [Id] FROM @Ids)
	AND ResourceClassificationId = dbo.fn_RCCode__Id(N'Cash')
	AND DefinitionId = N'currencies';

	DELETE FROM [dbo].Resources 
	WHERE Id IN (SELECT Id FROM @Ids);

	DELETE FROM dbo.[Currencies]
	WHERE [Id] IN (SELECT [Id] FROM @CurrenciesToDelete);
