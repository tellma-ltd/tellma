CREATE PROCEDURE [bll].[Currencies_Validate__Delete]
	@Ids [dbo].[IndexedStringList] READONLY,
	@TOP INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- The currency should not be used in Accounts
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT TOP (@TOP)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrency0IsUsedInAccount1', 
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS CurrencyName,
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS AccountName
	FROM @Ids FE
	JOIN [dbo].[Currencies] C ON C.[Id] = FE.[Id]
	JOIN dbo.Accounts A ON A.CurrencyId = FE.[Id];
	-- Currency must not be used in Resources
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT TOP (@TOP)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrency0IsUsedInResource1', 
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS CurrencyName,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS ResourceName
	FROM @Ids FE
	JOIN [dbo].[Currencies] C ON C.[Id] = FE.[Id]
	JOIN dbo.Resources R ON R.CurrencyId = FE.[Id];
	-- Currency must not be used in Entries
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
    SELECT TOP (@TOP)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrency0IsUsedInDocument1', 
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS CurrencyName,
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N]
	FROM @Ids FE
	JOIN [dbo].[Currencies] C ON C.[Id] = FE.[Id]
	JOIN dbo.Entries E ON E.CurrencyId = FE.[Id]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON L.[DocumentId] = D.[Id]
	JOIN dbo.DocumentDefinitions DD ON D.DefinitionId = DD.[Id]
	WHERE L.[State] > 0
	;

	SELECT TOP(@Top) * FROM @ValidationErrors;