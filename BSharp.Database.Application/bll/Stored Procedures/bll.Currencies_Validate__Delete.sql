CREATE PROCEDURE [bll].[Currencies_Validate__Delete]
	@Ids [dbo].[IndexedStringList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	-- TODO: we check if the corrsponding resources are used in entries or accounts. if they are, we return an error
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrency0IsUsedIn1', 
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS CurrencyName,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS ResourceName
    FROM [dbo].[Currencies] C
	JOIN [dbo].[Resources] R ON R.[CurrencyId] = C.Id
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