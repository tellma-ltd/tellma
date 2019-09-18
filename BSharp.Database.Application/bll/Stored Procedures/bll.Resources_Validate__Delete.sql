CREATE PROCEDURE [bll].[Resources_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors ([Key], [ErrorName], [Argument0], [Argument1])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_The01IsUsedInOneOrMoreDocuments',
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS ResourceDefinitionTitleSingular,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS ResourceName
	FROM [dbo].[Resources] R 
	JOIN [dbo].[ResourceDefinitions] RD ON R.ResourceDefinitionId = RD.Id
	JOIN @Ids FE ON FE.[Id] = R.[Id]
	WHERE R.[Id] IN (SELECT ResourceId FROM dbo.DocumentLineEntries);
	
	SELECT TOP(@Top) * FROM @ValidationErrors;