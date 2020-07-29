CREATE PROCEDURE [bll].[Custodies_Validate__Delete]	
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Cannot delete a Custody that is used in some documents
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3])
    SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCustody01IsUsedInDocument23', 
		[dbo].[fn_Localize](CD.[TitleSingular], CD.[TitleSingular2], CD.[TitleSingular3]) AS [CustodyDefinition],
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS [Custody],
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS [DocumentDefinition],
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [S/N]
    FROM [dbo].[Custodies] C
	JOIN [dbo].[CustodyDefinitions] CD ON C.[DefinitionId] = CD.[Id]
	JOIN [dbo].[Entries] E ON E.[CustodianId] = C.[Id]
	JOIN [dbo].[Lines] L ON L.[Id] =  E.[LineId]
	JOIN [dbo].[Documents] D ON D.[Id] = L.[DocumentId]
	JOIN [dbo].[DocumentDefinitions] DD ON DD.[Id] = D.[DefinitionId]
	JOIN @Ids FE ON FE.[Id] = C.[Id]
	
	-- Cannot delete a Custody that is used in some accounts
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
    SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCustody01IsUsedInAccount2', 
		[dbo].[fn_Localize](C.[Name], C.[Name2], C.[Name3]) AS [Custody],
		[dbo].[fn_Localize](CD.[TitleSingular], CD.[TitleSingular2], CD.[TitleSingular3]) AS [CustodyDefinition],
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS [Account]
    FROM [dbo].[Custodies] C
	JOIN [dbo].[CustodyDefinitions] CD ON C.[DefinitionId] = CD.[Id]
	JOIN [dbo].[Accounts] A ON A.[CustodianId] = C.[Id]
	JOIN @Ids FE ON FE.[Id] = C.[Id]

	SELECT TOP(@Top) * FROM @ValidationErrors;
