CREATE PROCEDURE [bll].[Documents_Validate__Post]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Document Date not before last archive date (C#)
	-- Posting date must not be within Archived period (C#)

	-- Cannot file with no lines
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasNoLines'
	FROM @Ids 
	WHERE [Id] NOT IN (
		SELECT DISTINCT [DocumentId] 
		FROM dbo.[Lines]
	);

	-- All lines must be in their final states.
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + '].Lines[' +
			CAST(D.[Id] AS NVARCHAR (255)) + ']',
		N'Error_State0IsNotFinal',
		dbo.fn_StateId__State(L.[State])
	FROM @Ids D
	JOIN dbo.[Lines] L ON L.[DocumentId] = D.[Id]
	--WHERE DL.[State] IN (N'Draft', N'Requested', N'Authorized', N'Completed')
	WHERE L.[State] Between 0 AND 3

	-- Cannot post a document which does not have at lease one line that is (Ready To Post)
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentDoesNotHaveAnyReadyToPostLines'
	FROM @Ids 
	WHERE [Id] NOT IN (
		SELECT DISTINCT [DocumentId] 
		FROM dbo.[Lines]
		WHERE [State] = 4
	);
	-- Cannot post a document with non-balanced (Ready to Post) lines
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TransactionHasDebitCreditDifference0',
		FORMAT(SUM(E.[Direction] * E.[Value]), '##,#;(##,#);-', 'en-us') AS NetDifference
	FROM @Ids FE
	JOIN dbo.[Lines] L ON FE.[Id] = L.[DocumentId]
	JOIN dbo.[Entries] E ON L.[Id] = E.[LineId]
	WHERE L.[State] = +4 -- N'Ready To Post'
	GROUP BY FE.[Index]
	HAVING SUM(E.[Direction] * E.[Value]) <> 0;

	SELECT TOP (@Top) * FROM @ValidationErrors;