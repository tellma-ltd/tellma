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
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS ResourceName
	FROM @Ids FE
	JOIN [dbo].[Currencies] C ON C.[Id] = FE.[Id]
	JOIN dbo.Accounts A ON A.CurrencyId = FE.[Id];

	-- TODO: Currency must not be used in Resources

	-- TODO: Currency must not be used in Entries


	SELECT TOP(@Top) * FROM @ValidationErrors;