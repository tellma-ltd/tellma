CREATE PROCEDURE [bll].[Lookups_Validate__Delete]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	
	-- Check that LookupId is not used in Resources
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheLookup0IsUsedInResource12',
		dbo.fn_Localize(L.[Name], L.[Name2], L.[Name3]) AS [Lookup],
		dbo.fn_Localize(RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS [ResourceDefinition],
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource]
	FROM @Ids FE
	JOIN dbo.[Lookups] L ON (L.[Id] = FE.[Id])
	JOIN dbo.[Resources] R ON (FE.[Id] = R.[Lookup1Id] OR FE.[Id] = R.[Lookup2Id])
	JOIN dbo.[ResourceDefinitions] RD ON RD.[Id] = R.[DefinitionId]

	SELECT TOP(@Top) * FROM @ValidationErrors;