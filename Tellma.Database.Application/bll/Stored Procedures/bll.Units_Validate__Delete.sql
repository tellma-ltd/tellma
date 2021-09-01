CREATE PROCEDURE [bll].[Units_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Make sure the unit is not in table Entries
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheUnit0IsUsedInDocument12',
		[dbo].[fn_Localize](U.[Name], U.[Name2], U.[Name3]) AS UnitName,
		[dbo].[fn_Localize](DD.[TitleSingular], DD.[TitleSingular2], DD.[TitleSingular3]) AS DocumentDefinition,
		DD.[Code]
    FROM @Ids FE
	JOIN dbo.[Units] U ON FE.[Id] = U.Id
	JOIN dbo.[Entries] E ON E.UnitId = FE.Id
	JOIN dbo.[Lines] L ON L.[Id] = E.[LineId]
	JOIN map.Documents() D ON L.[DocumentId] = D.[Id]
	JOIN dbo.DocumentDefinitions DD ON D.[DefinitionId] = DD.[Id]

	-- Make sure the unit is not in table Resources
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheUnit0IsUsedInResource12',
		[dbo].[fn_Localize](U.[Name], U.[Name2], U.[Name3]) AS UnitName,
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS ResourceDefinition,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS [Resource]
    FROM @Ids FE
	JOIN dbo.Units U ON FE.[Id] = U.Id
	JOIN dbo.Resources R ON R.UnitId = FE.Id
	JOIN dbo.ResourceDefinitions RD ON RD.[Id] = R.[DefinitionId]

	-- TODO: Make sure the unit is not in table Account Balances	

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;