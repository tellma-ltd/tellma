CREATE PROCEDURE [bll].[Resources_Validate__Delete]
	@DefinitionId NVARCHAR(255),
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors ([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_The01IsUsedInAccount2',
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS ResourceDefinitionTitleSingular,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS ResourceName,
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS AccountName
	FROM [dbo].[Resources] R 
	JOIN [dbo].[ResourceDefinitions] RD ON R.[DefinitionId] = RD.Id
	JOIN @Ids FE ON FE.[Id] = R.[Id]
	JOIN dbo.Accounts A ON A.ResourceId = R.Id;
	
	-- If the resource is a monetary resource, make sure the currency is not used in other resources
	WITH Currencies AS
	(
		SELECT I.[Index], R.CurrencyId, R.[Name], R.[Name2], R.[Name3] FROM dbo.Resources R
		JOIN @Ids I ON R.[Id] = I.[Id]
		WHERE R.ResourceClassificationId = dbo.fn_RCCode__Id(N'Cash')
		AND R.DefinitionId = N'currencies'
	)
	INSERT INTO @ValidationErrors ([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP(@Top)
		'[' + CAST(C.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrency0IsUsedInResource1',
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS CurrencyName,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS ResourceName
	FROM Currencies C 
	JOIN [dbo].[Resources] R ON R.[CurrencyId] = C.CurrencyId
	WHERE R.[Id] NOT IN (SELECT Id FROM @Ids);

	-- TODO: If the resource is a monetary resource, make sure the currency is not used in accounts

	-- TODO: If the resource is a monetary resource, make sure the currency is not used in entries

	SELECT TOP(@Top) * FROM @ValidationErrors;