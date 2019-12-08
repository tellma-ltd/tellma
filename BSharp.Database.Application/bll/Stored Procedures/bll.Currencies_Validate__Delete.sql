CREATE PROCEDURE [bll].[Currencies_Validate__Delete]
	@Ids [dbo].[IndexedStringList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- The currency resource should not be used in Accounts
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT TOP (@TOP)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrency0IsUsedInAccount1', 
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS CurrencyName,
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS ResourceName
	FROM @Ids FE
	JOIN [dbo].[Resources] R ON R.[CurrencyId] = FE.[Id]
	JOIN dbo.Accounts A ON A.ResourceId = R.[Id]
	WHERE R.[DefinitionId] = N'currencies'
	AND R.[ResourceClassificationId] = dbo.fn_RCCode__Id(N'Cash')

	-- TODO: The currency resource should not be used in entries

	-- The currency should not be used in other resources
	WITH CurrencyResources AS
	(
		SELECT [Id] FROM dbo.Resources
		WHERE [DefinitionId] = N'currencies'
		AND [ResourceClassificationId] = dbo.fn_RCCode__Id(N'Cash')
		AND [CurrencyId] IN (SELECT [Id] FROM @Ids)
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT TOP (@TOP)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrency0IsUsedIn1', 
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS CurrencyName,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS ResourceName
    FROM [dbo].[Currencies] C
	JOIN [dbo].[Resources] R ON R.[CurrencyId] = C.Id
	JOIN @Ids FE ON FE.[Id] = C.[Id]
	WHERE R.[Id] NOT IN (SELECT [Id] FROM CurrencyResources)

	SELECT TOP(@Top) * FROM @ValidationErrors;