CREATE PROCEDURE [bll].[Users_Validate__Save]
	@Entities [UserList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Email must not be already in the back end
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
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
	SELECT
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

	SELECT TOP(@Top) * FROM @ValidationErrors;