CREATE PROCEDURE [bll].[ResourceClassifications_Validate__Save]
	@Entities [dbo].[ResourceClassificationList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotModifyInactiveItem'
    FROM @Entities
    WHERE Id IN (SELECT Id from [dbo].[ResourceClassifications] WHERE IsActive = 0)
	OPTION(HASH JOIN);

    -- Non Null Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE Id <> 0
	AND Id NOT IN (SELECT Id from [dbo].[ResourceClassifications])
	OPTION(HASH JOIN);

	-- Code must not be already in the back end
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code AS Argument0
	FROM @Entities FE 
	JOIN [dbo].[ResourceClassifications] BE ON FE.Code = BE.Code
	WHERE FE.[Code] IS NOT NULL
	AND (FE.Id <> BE.Id)
	OPTION(HASH JOIN);

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
	) OPTION(HASH JOIN);
	SELECT TOP (@Top) * FROM @ValidationErrors;
