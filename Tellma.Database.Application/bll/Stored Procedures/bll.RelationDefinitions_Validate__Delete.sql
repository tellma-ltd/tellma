CREATE PROCEDURE [bll].[RelationDefinitions_Validate__Delete]
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
		dbo.fn_Localize(D.[TitlePlural], D.[TitlePlural2], D.[TitlePlural3]) AS [RelationDefinition]
	FROM @Ids FE
	JOIN dbo.[RelationDefinitions] D ON D.[Id] = FE.[Id]
	JOIN dbo.[Relations] R ON R.[DefinitionId] = FE.[Id]

	-- Check that Definition is not used in Account Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccountType1',
		dbo.fn_Localize(RD.[TitlePlural], RD.[TitlePlural2], RD.[TitlePlural3]) AS [RelationDefinition],
		dbo.fn_Localize(AC.[Name], AC.[Name2], AC.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN dbo.[AccountTypeRelationDefinitions] ATRD ON ATRD.[RelationDefinitionId] = FE.[Id]
	JOIN dbo.AccountTypes AC ON ATRD.AccountTypeId = AC.[Id]
	JOIN dbo.[RelationDefinitions] RD ON ATRD.[RelationDefinitionId] = RD.[Id]

	-- Check that Definition is not used in Account Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccountType1',
		dbo.fn_Localize(RD.[TitlePlural], RD.[TitlePlural2], RD.[TitlePlural3]) AS [NotedRelationDefinition],
		dbo.fn_Localize(AC.[Name], AC.[Name2], AC.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN dbo.[AccountTypeNotedRelationDefinitions] ATRD ON ATRD.[NotedRelationDefinitionId] = FE.[Id]
	JOIN dbo.AccountTypes AC ON ATRD.AccountTypeId = AC.[Id]
	JOIN dbo.[RelationDefinitions] RD ON ATRD.[NotedRelationDefinitionId] = RD.[Id]

	SELECT TOP(@Top) * FROM @ValidationErrors;