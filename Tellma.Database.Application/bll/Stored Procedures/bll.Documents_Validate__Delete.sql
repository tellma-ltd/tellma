﻿CREATE PROCEDURE [bll].[Documents_Validate__Delete]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Document Date not before last archive date (C#)
	-- Posting date must not be within Archived period (C#)

	-- Cannot delete unless in Draft state or negative states
	INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentIsClosed'
	FROM @Ids FE 
	JOIN [dbo].[Documents] D ON FE.[Id] = D.[Id]
	WHERE D.[State] = +1

	IF EXISTS(SELECT * FROM @ValidationErrors) GOTO DONE

	-- Cannot delete if it will cause a gap in the sequence of serial numbers
	IF (SELECT [IsOriginalDocument] FROM [dbo].[DocumentDefinitions] WHERE [Id] = @DefinitionId) = 1
	INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_ThereAreSubsequentDocuments'
	FROM @Ids FE 
	JOIN [dbo].[Documents] D ON FE.[Id] = D.[Id]
	JOIN [dbo].[Documents] DO ON D.[DefinitionId] = DO.[DefinitionId]
	JOIN [dbo].[DocumentDefinitions] DD ON D.[DefinitionId] = DD.[Id]
	WHERE DD.[IsOriginalDocument] = 1 
	AND D.[SerialNumber] < DO.[SerialNumber]
	AND DO.[Id] NOT IN (SELECT [Id] FROM @Ids)

	-- Cannot delete If there are approved lines
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasApprovedLines',
		CAST(L.[State] AS NVARCHAR(50))
	FROM @Ids FE 
	JOIN [dbo].[Lines] L ON FE.[Id] = L.[DocumentId]
	WHERE L.[State] >= 2

DONE:

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;