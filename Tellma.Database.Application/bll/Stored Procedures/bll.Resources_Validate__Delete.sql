CREATE PROCEDURE [bll].[Resources_Validate__Delete]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors ([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_The01IsUsedInAccount2',
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS ResourceDefinitionTitleSingular,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS ResourceName,
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS AccountName
	FROM [dbo].[Resources] R 
	JOIN [dbo].[ResourceDefinitions] RD ON R.[DefinitionId] = RD.[Id]
	JOIN @Ids FE ON FE.[Id] = R.[Id]
	JOIN dbo.Accounts A ON A.ResourceId = R.Id;

	INSERT INTO @ValidationErrors ([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
	SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_The01IsUsedInDocument23',
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS ResourceDefinitionTitleSingular,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS ResourceName,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinitionTitleSingular,
		D.Code
	FROM [dbo].[Resources] R 
	JOIN [dbo].[ResourceDefinitions] RD ON R.[DefinitionId] = RD.[Id]
	JOIN @Ids FE ON FE.[Id] = R.[Id]
	JOIN dbo.Entries E ON E.ResourceId = R.Id
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN map.Documents() D ON L.[DocumentId] = D.[Id]
	JOIN map.DocumentDefinitions() DD ON D.[DefinitionId] = DD.[Id]

	SELECT TOP(@Top) * FROM @ValidationErrors;