﻿CREATE PROCEDURE [bll].[PrintingTemplates_Validate__Save]
	@Entities [PrintingTemplateList] READONLY,
	@Parameters [dbo].[PrintingTemplateParameterList] READONLY,
	@Roles [dbo].[PrintingTemplateRoleList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

    -- Non Null Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Id',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE [Id] <> 0
	AND [Id] NOT IN (SELECT [Id] from [dbo].[PrintingTemplates]);

	-- Code must be unique
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0]) 
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
		N'Error_TheCode0IsUsed',
		FE.Code
	FROM @Entities FE 
	JOIN [dbo].[PrintingTemplates] BE ON FE.[Code] = BE.[Code]
	WHERE ((FE.[Id] IS NULL) OR (FE.[Id] <> BE.[Id]));

	-- Code must not be duplicated in the uploaded list (Depends on SQL collation)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
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
	)
	
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;