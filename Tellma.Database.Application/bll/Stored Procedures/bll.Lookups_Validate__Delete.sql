CREATE PROCEDURE [bll].[Lookups_Validate__Delete]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Check all deleted items are consistent with @DefinitionId
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheLookupIs1',
		dbo.fn_Localize(LD.[TitleSingular], LD.[TitleSingular2], LD.[TitleSingular3]) AS [LookupDefinition]
	FROM @Ids FE
	JOIN dbo.Lookups L ON L.[Id] = FE.[Id]
	JOIN dbo.LookupDefinitions LD ON LD.[Id] = L.[DefinitionId]
	WHERE L.[DefinitionId] <> @DefinitionId
	
	-- Check that LookupId is not used in Resources
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheLookupIsUsedInResource01',
		dbo.fn_Localize(RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS [ResourceDefinition],
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS [Resource]
	FROM @Ids FE
	JOIN dbo.Resources R ON (FE.[Id] = R.[Lookup1Id] OR FE.[Id] = R.[Lookup2Id])
	JOIN dbo.ResourceDefinitions RD ON RD.[Id] = R.[DefinitionId]

	SELECT TOP(@Top) * FROM @ValidationErrors;