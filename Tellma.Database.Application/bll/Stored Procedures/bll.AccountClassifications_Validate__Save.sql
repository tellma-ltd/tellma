CREATE PROCEDURE [bll].[AccountClassifications_Validate__Save]
	@Entities [AccountClassificationList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	
    -- Non zero Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Id',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE [Id] <> 0
	AND [Id] NOT IN (SELECT [Id] from [dbo].[AccountClassifications]);

		-- Code must not be already in the back end
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code AS Argument0
	FROM @Entities FE 
	JOIN [dbo].[AccountClassifications] BE ON FE.[Code] = BE.[Code]
	WHERE FE.[Id] <> BE.[Id];

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

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	-- Return Errors
	SELECT TOP(@Top) * FROM @ValidationErrors;
END;