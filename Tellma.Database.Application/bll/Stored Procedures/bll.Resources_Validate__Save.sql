CREATE PROCEDURE [bll].[Resources_Validate__Save]
	@DefinitionId INT,
	@Entities [dbo].[ResourceList] READONLY,
	@ResourceUnits dbo.ResourceUnitList READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT TOP(@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotModifyInactiveItem'
    FROM @Entities
    WHERE Id IN (SELECT Id from [dbo].[Resources] WHERE IsActive = 0);

    -- Non zero Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT TOP(@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE Id <> 0 AND Id NOT IN (SELECT Id from [dbo].[Resources])

	-- Code must be unique
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.Code = BE.Code AND BE.[DefinitionId] = @DefinitionId
	WHERE (FE.Id <> BE.Id);

	-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
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
	SELECT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name',
		N'Error_TheName0IsUsed',
		FE.[Name]
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.[Name] = BE.[Name] AND FE.[Identifier] = BE.[Identifier]
	WHERE BE.DefinitionId = @DefinitionId AND  (FE.Id <> BE.Id);

	-- Name2 must not exist in the db
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name2',
		N'Error_TheName0IsUsed',
		FE.[Name2]
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.[Name2] = BE.[Name2] AND FE.[Identifier] = BE.[Identifier]
	WHERE BE.[DefinitionId] = @DefinitionId AND (FE.Id <> BE.Id);

	-- Name3 must not exist in the db
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name3',
		N'Error_TheName0IsUsed',
		FE.[Name3]
	FROM @Entities FE 
	JOIN [dbo].[Resources] BE ON FE.[Name3] = BE.[Name3] AND FE.[Identifier] = BE.[Identifier]
	WHERE BE.[DefinitionId] = @DefinitionId AND (FE.Id <> BE.Id);

	-- Name must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name',
		N'Error_TheName0IsDuplicated',
		[Name]
	FROM @Entities
	WHERE [Name] IN (
		SELECT [Name]
		FROM @Entities
		GROUP BY [Name], [Identifier]
		HAVING COUNT(*) > 1
	);

	-- Name2 must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name2',
		N'Error_TheName0IsDuplicated',
		[Name2]
	FROM @Entities
	WHERE [Name2] IN (
		SELECT [Name2]
		FROM @Entities
		WHERE [Name2] IS NOT NULL
		GROUP BY [Name2], [Identifier]
		HAVING COUNT(*) > 1
	);

	-- Name3 must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name3',
		N'Error_TheName0IsDuplicated',
		[Name3]
	FROM @Entities
	WHERE [Name3] IN (
		SELECT [Name3]
		FROM @Entities
		WHERE [Name3] IS NOT NULL
		GROUP BY [Name3], [Identifier]
		HAVING COUNT(*) > 1
	);

	-- Unit in ResourceUnits must be of same type of Header unit or be of type Mass
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT TOP(@Top)
		'[' + CAST(R.[Index] AS NVARCHAR (255)) + '].Units[' + CAST(RU.[Index] AS NVARCHAR(255)) + '].UnitId',
		N'Error_TheUnit0HasIncompatibleUnitTypeMustBeType1',
		[dbo].[fn_Localize](URU.[Name], URU.[Name2], URU.[Name3]) AS [NameOfIncompatibleUnitName],
		N'localize:Unit_' + UR.[UnitType] as [ExpectedType]
	FROM @Entities R
	JOIN dbo.Units UR ON R.[UnitId] = UR.[Id]
	JOIN @ResourceUnits RU ON R.[Index] = RU.[HeaderIndex]
	JOIN dbo.Units URU ON RU.[UnitId] = URU.[Id]
	WHERE URU.[UnitType] <> N'Mass'
	AND URU.[UnitType] <> UR.[UnitType]

	SELECT TOP (@Top) * FROM @ValidationErrors;
