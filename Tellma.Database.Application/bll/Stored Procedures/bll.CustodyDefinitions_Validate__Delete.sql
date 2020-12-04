CREATE PROCEDURE [bll].[CustodyDefinitions_Validate__Delete]
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
		dbo.fn_Localize(D.[TitlePlural], D.[TitlePlural2], D.[TitlePlural3]) AS [Custody]
	FROM @Ids FE
	JOIN dbo.[CustodyDefinitions] D ON D.[Id] = FE.[Id]
	JOIN dbo.[Custodies] R ON R.[DefinitionId] = FE.[Id]

	-- Check that Definition is not used in Account Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP (@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCustodyDefinition0IsUsedInAccountType1',
		dbo.fn_Localize(D.[TitleSingular], D.[TitleSingular2], D.[TitleSingular3]) AS [Definition],
		dbo.fn_Localize(AD.[Name], AD.[Name2], AD.[Name3]) AS [AccountType]
	FROM @Ids FE
	JOIN dbo.[CustodyDefinitions] D ON D.[Id] = FE.[Id]
	JOIN dbo.[AccountTypeCustodyDefinitions] ADRD ON ADRD.[CustodyDefinitionId] = FE.[Id]
	JOIN dbo.[AccountTypes] AD ON AD.[Id] = ADRD.[AccountTypeId]

	-- Check that Definition is not used in Accounts
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP (@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCustodyDefinition0IsUsedInAccount1',
		dbo.fn_Localize(D.[TitleSingular], D.[TitleSingular2], D.[TitleSingular3]) AS [Definition],
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]) AS [Account]
	FROM @Ids FE
	JOIN dbo.[CustodyDefinitions] D ON D.[Id] = FE.[Id]
	JOIN dbo.[Accounts] A ON A.[CustodyDefinitionId] = D.[Id]

	SELECT TOP(@Top) * FROM @ValidationErrors;