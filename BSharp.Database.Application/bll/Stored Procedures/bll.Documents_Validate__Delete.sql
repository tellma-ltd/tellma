CREATE PROCEDURE [bll].[Documents_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Document Date not before last archive date (C#)
	-- Posting date must not be within Archived period (C#)

	-- Cannot delete unless in Draft state
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].State',
		N'Error_TheDocumentState0IsNotDraft',
		CAST([State] AS NVARCHAR(50))
	FROM @Ids 
	WHERE [Id] IN (
		SELECT [Id] 
		FROM dbo.[Documents]
		WHERE [State] > 0
	);

	-- Cannot delete If there are completed lines
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHascompletedLines',
		CAST([State] AS NVARCHAR(50))
	FROM @Ids 
	WHERE [Id] IN (
		SELECT DISTINCT [DocumentId] 
		FROM dbo.[Lines]
		WHERE [State] >= 3
	);


	SELECT TOP (@Top) * FROM @ValidationErrors;