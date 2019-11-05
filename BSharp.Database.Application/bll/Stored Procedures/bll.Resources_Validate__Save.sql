CREATE PROCEDURE [bll].[Resources_Validate__Save]
	@DefinitionId NVARCHAR (255),
	@Entities [dbo].[ResourceList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotModifyInactiveItem'
    FROM @Entities
    WHERE Id IN (SELECT Id from [dbo].[Resources] WHERE IsActive = 0);

    -- Non zero Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE Id <> 0 AND Id NOT IN (SELECT Id from [dbo].[Resources])

	-- Code must be unique
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.Code = BE.Code AND BE.ResourceDefinitionId = @DefinitionId
	WHERE (FE.Id <> BE.Id);

	-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
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

	-- Name must not exist in the db
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT 
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name',
		N'Error_TheName0IsUsed',
		FE.[Name]
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.[Name] = BE.[Name] AND BE.ResourceDefinitionId = @DefinitionId
	WHERE (FE.Id <> BE.Id);

	-- Name2 must not exist in the db
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name2',
		N'Error_TheName0IsUsed',
		FE.[Name2]
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.[Name2] = BE.[Name2] AND BE.ResourceDefinitionId = @DefinitionId
	WHERE (FE.Id <> BE.Id);

	-- Name3 must not exist in the db
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name3',
		N'Error_TheName0IsUsed',
		FE.[Name3]
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.[Name3] = BE.[Name3] AND BE.ResourceDefinitionId = @DefinitionId
	WHERE (FE.Id <> BE.Id);

	-- Name must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name',
		N'Error_TheName0IsDuplicated',
		[Name]
	FROM @Entities
	WHERE [Name] IN (
		SELECT [Name]
		FROM @Entities
		GROUP BY [Name]
		HAVING COUNT(*) > 1
	);

	-- Name2 must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name2',
		N'Error_TheName0IsDuplicated',
		[Name2]
	FROM @Entities
	WHERE [Name2] IN (
		SELECT [Name2]
		FROM @Entities
		WHERE [Name2] IS NOT NULL
		GROUP BY [Name2]
		HAVING COUNT(*) > 1
	);

	-- Name3 must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name3',
		N'Error_TheName0IsDuplicated',
		[Name3]
	FROM @Entities
	WHERE [Name3] IN (
		SELECT [Name3]
		FROM @Entities
		WHERE [Name3] IS NOT NULL
		GROUP BY [Name3]
		HAVING COUNT(*) > 1
	);

	SELECT TOP (@Top) * FROM @ValidationErrors;
