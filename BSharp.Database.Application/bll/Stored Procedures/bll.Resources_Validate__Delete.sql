CREATE PROCEDURE [bll].[Resources_Validate__Delete]
	@DefinitionId NVARCHAR(255),
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors ([Key], [ErrorName], [Argument0], [Argument1])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_The01IsUsedInOneOrMoreAccounts',
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS ResourceDefinitionTitleSingular,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS ResourceName
	FROM [dbo].[Resources] R 
	JOIN [dbo].[ResourceDefinitions] RD ON R.[DefinitionId] = RD.Id
	JOIN @Ids FE ON FE.[Id] = R.[Id]
	WHERE R.[Id] IN (SELECT ResourceId FROM dbo.Accounts);
	
	-- If the resource is a monetary resource, make sure the currency is not used in other resources
	WITH Currencies AS
	(
		SELECT I.[Index], R.CurrencyId, R.[Name], R.[Name2], R.[Name3] FROM dbo.Resources R
		JOIN @Ids I ON R.[Id] = I.[Id]
		WHERE R.ResourceClassificationId = dbo.fn_RCCode__Id(N'Cash')
		AND R.DefinitionId = N'monetary-resources'
	)
	INSERT INTO @ValidationErrors ([Key], [ErrorName], [Argument0], [Argument1])
	SELECT
		'[' + CAST(C.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrency01IsUsedInOneOrMoreResources',
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS CurrencyName,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS ResourceName
	FROM Currencies C 
	JOIN [dbo].[Resources] R ON R.[CurrencyId] = C.CurrencyId
	WHERE R.[Id] NOT IN (SELECT Id FROM @Ids);

	-- TODO: If the resource is a monetary resource, make sure the currency is not used in accounts

	-- TODO: If the resource is a monetary resource, make sure the currency is not used in entries

	SELECT TOP(@Top) * FROM @ValidationErrors;