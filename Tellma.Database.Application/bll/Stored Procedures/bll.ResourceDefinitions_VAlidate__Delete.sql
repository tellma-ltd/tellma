CREATE PROCEDURE [bll].[ResourceDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Check that ResourceDefinitionId is not used
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0AlreadyContainsData',
		[dbo].[fn_Localize](RD.[TitlePlural], RD.[TitlePlural2], RD.[TitlePlural3]) AS [Resource]
	FROM @Ids FE
	JOIN [dbo].[ResourceDefinitions] RD ON RD.[Id] = FE.[Id]
	JOIN [dbo].[Resources] R ON R.[DefinitionId] = FE.[Id]

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccountType1',
		[dbo].[fn_Localize](RD.[TitlePlural], RD.[TitlePlural2], RD.[TitlePlural3]) AS [Resource],
		[dbo].[fn_Localize](AC.[Name], AC.[Name2], AC.[Name3]) AS [AccountType]
	FROM @Ids FE
	JOIN [dbo].[AccountTypeResourceDefinitions] ATRD ON ATRD.[ResourceDefinitionId] = FE.[Id]
	JOIN [dbo].[AccountTypes] AC ON ATRD.[AccountTypeId] = AC.[Id]
	JOIN [dbo].[ResourceDefinitions] RD ON ATRD.[ResourceDefinitionId] = RD.[Id]

	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccount1',
		[dbo].[fn_Localize](RD.[TitlePlural], RD.[TitlePlural2], RD.[TitlePlural3]) AS [Resource],
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS [AccountType]
	FROM @Ids FE
	JOIN [dbo].[Accounts] A ON A.[ResourceDefinitionId] = FE.[Id]
	JOIN [dbo].[ResourceDefinitions] RD ON FE.[Id] = RD.[Id]

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;