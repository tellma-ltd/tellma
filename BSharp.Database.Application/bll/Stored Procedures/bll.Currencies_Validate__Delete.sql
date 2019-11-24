CREATE PROCEDURE [bll].[Currencies_Validate__Delete]
	@Ids [dbo].[IndexedStringList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
    SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrency0IsUsedIn12', 
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS CurrencyName,
		[dbo].[fn_Localize](AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS AccountDefinitionTitleSingular,
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS ResourceName
    FROM [dbo].[Currencies] C
	JOIN [dbo].Accounts A ON A.[CurrencyId] = C.Id
	JOIN dbo.AccountDefinitions AD ON A.AccountDefinitionId = AD.Id
	JOIN @Ids FE ON FE.[Id] = C.[Id];

	---- TODO: Is it really used in [dbo].[Accounts]?
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
 --   SELECT
	--	'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
	--	N'Error_TheCurrency0IsUsedInAccount1', 
	--	[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS CurrencyName,
	--	[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS AccountName
 --   FROM [dbo].[Currencies] C
	--JOIN [dbo].[Accounts] A ON A.???Id = C.Id
	--JOIN @Ids FE ON FE.[Id] = C.[Id];

	SELECT TOP(@Top) * FROM @ValidationErrors;