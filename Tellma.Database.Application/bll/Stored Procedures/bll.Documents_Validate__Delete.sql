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
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentIsPosted'
	FROM @Ids FE 
	JOIN dbo.[Documents] D ON FE.[Id] = D.[Id]
	WHERE D.[State] = +1

	-- Cannot delete If there are completed lines
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHascompletedLines',
		CAST(L.[State] AS NVARCHAR(50))
	FROM @Ids FE 
	JOIN dbo.[Lines] L ON FE.[Id] = L.[DocumentId]
	WHERE L.[State] >= 3


	SELECT TOP (@Top) * FROM @ValidationErrors;