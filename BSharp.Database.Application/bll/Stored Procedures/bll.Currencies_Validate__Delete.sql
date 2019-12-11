CREATE PROCEDURE [bll].[Currencies_Validate__Delete]
	@Ids [dbo].[IndexedStringList] READONLY,
	@TOP INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- The currency resource should not be used in Accounts
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT TOP (@TOP)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrency0IsUsedInAccount1', 
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS CurrencyName,
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS ResourceName
	FROM @Ids FE
	JOIN [dbo].[Currencies] C ON C.[Id] = FE.[Id]
	JOIN dbo.Accounts A ON A.CurrencyId = FE.[Id];

	-- The currency resource should not be used in entries
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
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS ResourceName,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N]
    FROM [dbo].[Entries] E
	JOIN dbo.Lines L ON L.Id = E.LineId
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN [dbo].[Resources] R ON R.[Id] = E.ResourceId
	JOIN @Ids FE ON FE.[Id] = R.[CurrencyId]
	WHERE R.[Id] IN (SELECT [Id] FROM CurrencyResources);

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