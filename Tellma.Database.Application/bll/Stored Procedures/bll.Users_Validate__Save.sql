CREATE PROCEDURE [bll].[Users_Validate__Save]
	@Entities [UserList] READONLY,
	@Roles [dbo].[RoleMembershipList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
SET NOCOUNT ON;

	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- TODO: Make sure all non-null, non-zero User Ids exist in the database
	-- TODO: Make sure all non-null, non-zero Role Membership Ids exist in the database

	-- Email must not be already in the back end
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Email',
		N'Error_TheEmail0IsUsed',
		FE.[Email] AS Argument0
	FROM @Entities FE 
	JOIN [dbo].[Users] BE ON FE.[Email] = BE.[Email]
	WHERE
		FE.[Email] IS NOT NULL
	AND BE.[Email] IS NOT NULL
	AND FE.Id <> BE.Id
	OPTION (HASH JOIN);

	-- Email must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Email',
		N'Error_TheEmail0IsDuplicated',
		[Email]
	FROM @Entities
	WHERE [Email] IN (
		SELECT [Email]
		FROM @Entities
		WHERE [Email] IS NOT NULL
		GROUP BY [Email]
		HAVING COUNT(*) > 1
	) OPTION (HASH JOIN);

	-- No non existing roles
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1]) 
	SELECT TOP (@Top)
		'[' + CAST(P.[HeaderIndex] AS NVARCHAR(255)) + '].Roles[' + 
		CAST(P.[Index] AS NVARCHAR(255)) + '].RoleId' As [Key], N'Error_TheRole0IsNonExistent' As [ErrorName],
		P.[RoleId] AS Argument1
	FROM @Roles P
	WHERE P.RoleId NOT IN (
		SELECT [Id] FROM dbo.[Roles]
	);

	-- No inactive roles
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument1]) 
	SELECT TOP(@Top) 
		'[' + CAST(P.[HeaderIndex] AS NVARCHAR(255)) + '].Roles[' + 
		CAST(P.[Index] AS NVARCHAR(255)) + '].RoleId' As [Key],
		N'Error_TheRole0IsInactive' As [ErrorName],
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS RoleName
	FROM @Roles P JOIN [dbo].[Roles] R ON P.RoleId = R.Id
	WHERE R.IsActive = 0;

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	-- Return Errors
	SELECT TOP(@Top) * FROM @ValidationErrors;
END;