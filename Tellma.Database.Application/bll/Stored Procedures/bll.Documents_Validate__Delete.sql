CREATE PROCEDURE [bll].[Documents_Validate__Delete]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Document Date not before last archive date (C#)
	-- Posting date must not be within Archived period (C#)

	-- Cannot delete unless in Draft state or negative states
	INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentIsClosed'
	FROM @Ids FE 
	JOIN dbo.[Documents] D ON FE.[Id] = D.[Id]
	WHERE D.[State] = +1

	IF EXISTS(SELECT * FROM @ValidationErrors) GOTO DONE

	-- Cannot delete if it will cause a gap in the sequence of serial numbers
	INSERT INTO @ValidationErrors([Key], [ErrorName])
    SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_ThereAreSubsequentDocuments'
	FROM @Ids FE 
	JOIN dbo.[Documents] D ON FE.[Id] = D.[Id]
	JOIN dbo.[Documents] DO ON D.DefinitionId = DO.DefinitionId
	WHERE D.SerialNumber < DO.SerialNumber

	-- Cannot delete If there are completed lines
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasCompletedLines',
		CAST(L.[State] AS NVARCHAR(50))
	FROM @Ids FE 
	JOIN dbo.[Lines] L ON FE.[Id] = L.[DocumentId]
	WHERE L.[State] >= 3

DONE:
	SELECT TOP (@Top) * FROM @ValidationErrors;