CREATE PROCEDURE [bll].[ResourceDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Check that ResourceDefinitionId is not used
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0AlreadyContainsData',
		dbo.fn_Localize(RD.[TitlePlural], RD.[TitlePlural2], RD.[TitlePlural3]) AS [Resource]
	FROM @Ids FE
	JOIN dbo.ResourceDefinitions RD ON RD.[Id] = FE.[Id]
	JOIN dbo.Resources R ON R.[DefinitionId] = FE.[Id]

	SELECT TOP(@Top) * FROM @ValidationErrors;