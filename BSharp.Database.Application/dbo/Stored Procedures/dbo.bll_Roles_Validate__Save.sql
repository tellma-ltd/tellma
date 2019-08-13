CREATE PROCEDURE [dbo].[bll_Roles_Validate__Save]
	@Roles [dbo].[RoleList] READONLY,
	@Permissions [dbo].[PermissionList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

    INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].IsActive',
		N'Error_CannotModifyInactiveItem'
    FROM @Roles
    WHERE Id IN (SELECT Id from [dbo].[Roles] WHERE IsActive = 0)
	OPTION(HASH JOIN);

    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Id',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Roles
    WHERE Id Is NOT NULL
	AND Id NOT IN (SELECT Id from [dbo].[Roles])
	OPTION(HASH JOIN);
		
	-- Code must not be already in the back end
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code
	FROM @Roles FE
	JOIN [dbo].Roles BE ON FE.Code = BE.Code
	WHERE FE.[Code] IS NOT NULL
	AND (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

	-- Code must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsDuplicated',
		[Code]
	FROM @Roles
	WHERE [Code] IN (
		SELECT [Code]
		FROM @Roles
		WHERE [Code] IS NOT NULL
		GROUP BY [Code]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

	-- Name must not exist in the db
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT 
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name',
		N'Error_TheName0IsUsed',
		FE.[Name]
	FROM @Roles FE
	JOIN [dbo].Roles BE ON FE.[Name] = BE.[Name]
	WHERE (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

	-- Name2 must not exist in the db
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name2',
		N'Error_TheName0IsUsed',
		FE.[Name2]
	FROM @Roles FE
	JOIN [dbo].Roles BE ON FE.[Name2] = BE.[Name2]
	WHERE (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

	-- Name3 must not exist in the db
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Name3',
		N'Error_TheName0IsUsed',
		FE.[Name3]
	FROM @Roles FE
	JOIN [dbo].Roles BE ON FE.[Name3] = BE.[Name3]
	WHERE (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

	-- Name must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name',
		N'Error_TheName0IsDuplicated',
		[Name]
	FROM @Roles
	WHERE [Name] IN (
		SELECT [Name]
		FROM @Roles
		GROUP BY [Name]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

	-- Name2 must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name2',
		N'Error_TheName0IsDuplicated',
		[Name2]
	FROM @Roles
	WHERE [Name2] IN (
		SELECT [Name2]
		FROM @Roles
		WHERE [Name2] IS NOT NULL
		GROUP BY [Name2]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

	-- Name3 must be unique in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Name3',
		N'Error_TheName0IsDuplicated',
		[Name3]
	FROM @Roles
	WHERE [Name3] IN (
		SELECT [Name3]
		FROM @Roles
		WHERE [Name3] IS NOT NULL
		GROUP BY [Name3]
		HAVING COUNT(*) > 1
	) OPTION(HASH JOIN);

	-- No inactive view
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(P.[HeaderIndex] AS NVARCHAR (255)) + '].Permissions[' + 
				CAST(P.[Index] AS NVARCHAR (255)) + '].ViewId',
		N'Error_TheView0IsInactive',
		P.[ViewId]
	FROM @Permissions P
	WHERE (
		P.ViewId NOT IN (SELECT [Id] FROM dbo.[Views] WHERE IsActive = 1) OR 
		P.ViewId = N'All'
	);

	SELECT TOP (@Top) * FROM @ValidationErrors;