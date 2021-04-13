CREATE PROCEDURE [bll].[AccountTypes_Validate__Save]
	@Entities [dbo].[AccountTypeList] READONLY,
	@AccountTypeRelationDefinitions AccountTypeRelationDefinitionList READONLY,
	@AccountTypeResourceDefinitions AccountTypeResourceDefinitionList READONLY,
	@AccountTypeNotedRelationDefinitions AccountTypeRelationDefinitionList READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotModifyInactiveItem'
    FROM @Entities
    WHERE Id IN (SELECT Id from [dbo].[AccountTypes] WHERE IsActive = 0)
	OPTION(HASH JOIN);

    -- Non Null Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE Id <> 0
	AND Id NOT IN (SELECT Id from [dbo].[AccountTypes])
	OPTION(HASH JOIN);

	-- Code must not be already in the back end
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code AS Argument0
	FROM @Entities FE 
	JOIN [dbo].[AccountTypes] BE ON FE.Code = BE.Code
	WHERE FE.[Code] IS NOT NULL
	AND (FE.Id <> BE.Id);

	-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsDuplicated',
		[Code]
	FROM @Entities
	WHERE [Code] IN (
		SELECT [Code]
		FROM @Entities
		WHERE [Code] IS NOT NULL
		GROUP BY [Code]
		HAVING COUNT(*) > 1
	);

	-- Name must not be already in the back end
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name',
		N'Error_TheName0IsUsed',
		FE.[Name] AS Argument0
	FROM @Entities FE 
	JOIN [dbo].[AccountTypes] BE ON FE.[Name] = BE.[Name]
	WHERE FE.[Name] IS NOT NULL
	AND (FE.Id <> BE.Id);

	-- Name must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name',
		N'Error_TheName0IsDuplicated',
		[Name]
	FROM @Entities
	WHERE [Name] IN (
		SELECT [Name]
		FROM @Entities
		WHERE [Name] IS NOT NULL
		GROUP BY [Name]
		HAVING COUNT(*) > 1
	);

	SELECT TOP (@Top) * FROM @ValidationErrors;