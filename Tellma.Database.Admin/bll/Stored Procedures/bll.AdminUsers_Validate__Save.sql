CREATE PROCEDURE [bll].[AdminUsers_Validate__Save]
	@Entities [dbo].[AdminUserList] READONLY,
	@Permissions [dbo].[AdminPermissionList] READONLY,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Email must not be already in the back end
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT DISTINCT TOP(@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Email',
		N'Error_TheEmail0IsUsed',
		FE.[Email] AS Argument0
	FROM @Entities FE 
	JOIN [dbo].[AdminUsers] BE ON FE.[Email] = BE.[Email]
	WHERE
		FE.[Email] IS NOT NULL
	AND BE.[Email] IS NOT NULL
	AND FE.Id <> BE.Id;

	-- Email must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT DISTINCT TOP(@Top)
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
	);

	-- ClientId must not be already in the back end
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].ClientId',
		N'Error_TheClientId0IsUsed',
		FE.[ClientId] AS Argument0
	FROM @Entities FE 
	JOIN [dbo].[AdminUsers] BE ON FE.[ClientId] = BE.[ClientId]
	WHERE FE.[IsService] = 1 AND
		FE.[ClientId] IS NOT NULL
	AND BE.[ClientId] IS NOT NULL
	AND FE.[Id] <> BE.[Id];

	-- ClientId must not be duplicated in the uploaded list
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].ClientId',
		N'Error_TheClientId0IsDuplicated',
		[ClientId]
	FROM @Entities
	WHERE [IsService] = 0 AND [ClientId] IN (
		SELECT [ClientId]
		FROM @Entities
		WHERE [ClientId] IS NOT NULL
		GROUP BY [ClientId]
		HAVING COUNT(*) > 1
	);

	-- TODO: Check that the user is not modifying their own administrator permissions

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;