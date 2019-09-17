CREATE PROCEDURE [bll].[ResourceLookups_Validate__Save]
	@DefinitionId NVARCHAR(255),
	@Entities [ResourceLookupList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO: Ensure that @DefinitionId is valid and active
	-- TODO: Validate that definition ID of existing items matches @DefinitionId

	INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotModifyInactiveItem'
    FROM @Entities
    WHERE Id IN (SELECT Id from [dbo].[ResourceLookups] WHERE IsActive = 0)
	OPTION(HASH JOIN);

    -- Non Null Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE Id <> 0
	AND Id NOT IN (SELECT Id from [dbo].[ResourceLookups])
	OPTION(HASH JOIN);

		-- Code must not be already in the back end
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code AS Argument0
	FROM @Entities FE 
	JOIN [dbo].[ResourceLookups] BE ON FE.Code = BE.Code
	WHERE
		FE.[Code] IS NOT NULL
	AND BE.[Code] IS NOT NULL
	AND FE.Id <> BE.Id
	OPTION (HASH JOIN);

	-- Name must not exist in the db
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT 
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name',
		N'Error_TheName0IsUsed',
		FE.[Name]
	FROM @Entities FE 
	JOIN [dbo].[ResourceLookups] BE ON FE.[Name] = BE.[Name]
	WHERE (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

	-- Name2 must not exist in the db
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name2',
		N'Error_TheName0IsUsed',
		FE.[Name2]
	FROM @Entities FE 
	JOIN [dbo].[ResourceLookups] BE ON FE.[Name2] = BE.[Name2]
	WHERE (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

	-- Name3 must not exist in the db
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name3',
		N'Error_TheName0IsUsed',
		FE.[Name3]
	FROM @Entities FE 
	JOIN [dbo].[ResourceLookups] BE ON FE.[Name3] = BE.[Name3]
	WHERE (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

	-- These do not require access to the DB and therefore better handled by C#

	---- Name must be unique in the uploaded list
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	--SELECT
	--	'[' + CAST([Index] AS NVARCHAR (255)) + '].Name',
	--	N'Error_TheName0IsDuplicated',
	--	[Name]
	--FROM @Entities
	--WHERE [Name] IN (
	--	SELECT [Name]
	--	FROM @Entities
	--	GROUP BY [Name]
	--	HAVING COUNT(*) > 1
	--) OPTION(HASH JOIN);

	---- Name2 must be unique in the uploaded list
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	--SELECT
	--	'[' + CAST([Index] AS NVARCHAR (255)) + '].Name2',
	--	N'Error_TheName0IsDuplicated',
	--	[Name2]
	--FROM @Entities
	--WHERE [Name2] IN (
	--	SELECT [Name2]
	--	FROM @Entities
	--	WHERE [Name2] IS NOT NULL
	--	GROUP BY [Name2]
	--	HAVING COUNT(*) > 1
	--) OPTION(HASH JOIN);

	---- Name3 must be unique in the uploaded list
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	--SELECT
	--	'[' + CAST([Index] AS NVARCHAR (255)) + '].Name3',
	--	N'Error_TheName0IsDuplicated',
	--	[Name3]
	--FROM @Entities
	--WHERE [Name3] IN (
	--	SELECT [Name3]
	--	FROM @Entities
	--	WHERE [Name3] IS NOT NULL
	--	GROUP BY [Name3]
	--	HAVING COUNT(*) > 1
	--) OPTION(HASH JOIN);

	SELECT TOP (@Top) * FROM @ValidationErrors;
