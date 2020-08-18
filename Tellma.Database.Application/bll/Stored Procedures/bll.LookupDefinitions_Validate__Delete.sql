CREATE PROCEDURE [bll].[LookupDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Check that Definition is not used
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT DISTINCT TOP (@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0AlreadyContainsData',
		dbo.fn_Localize(D.[TitlePlural], D.[TitlePlural2], D.[TitlePlural3]) AS [Lookup]
	FROM @Ids FE
	JOIN dbo.[LookupDefinitions] D ON D.[Id] = FE.[Id]
	JOIN dbo.[Lookups] R ON R.[DefinitionId] = FE.[Id]

	SELECT TOP(@Top) * FROM @ValidationErrors;