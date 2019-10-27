CREATE PROCEDURE [bll].[Documents_Validate__File]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Document Date not before last archive date
	-- Posting date must not be within Archived period

	-- Cannot file with no lines
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasNoLines'
	FROM @Ids 
	WHERE [Id] NOT IN (
		SELECT DISTINCT [DocumentId] 
		FROM dbo.DocumentLines
	);

	-- All lines must be in their final states.
	-- TODO: convert State to integers
	WITH DocumentsLineDefinitions AS
	(
		SELECT DISTINCT DL.LineDefinitionId FROM 
		dbo.DocumentLines DL
		JOIN @Ids D ON DL.DocumentId = D.[Id]
	),
	WorkflowsFinalStateIds AS
	(
		SELECT LineDefinitionId, MAX([dbo].[fn_State__StateId]([ToState])) AS FinalStateId
		FROM dbo.Workflows
		WHERE LineDefinitionId IN (SELECT LineDefinitionId FROM DocumentsLineDefinitions)
		GROUP BY LineDefinitionId
	),
	WorkflowsFinalStates AS
	(
		SELECT LineDefinitionId, [dbo].[fn_StateId__State](FinalStateId) AS FinalState
		FROM WorkflowsFinalStateIds
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].DocumentLines[' +
			CAST(DL.[Id] AS NVARCHAR (255)) + ']',
		N'Error_State2IsNotFinal',
		DL.[State]
	FROM @Ids D
	JOIN dbo.DocumentLines DL ON DL.[DocumentId] = D.[Id]
	JOIN WorkflowsFinalStates WFS ON DL.[LineDefinitionId] = WFS.[LineDefinitionId]
	WHERE DL.[State] NOT IN (N'Void', N'Rejected', N'Failed', N'Invalid', WFS.FinalState)

	-- Cannot file a document with non-balanced (Reviewed) lines
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT
		'[' + ISNULL(CAST(FE.[Index] AS NVARCHAR (255)),'') + ']', 
		N'Error_TransactionHasDebitCreditDifference0',
		SUM([Direction] * [Value])
	FROM @Ids FE
	JOIN dbo.[DocumentLines] DL ON FE.[Id] = DL.[DocumentId]
	JOIN dbo.[DocumentLineEntries] DLE ON DL.[Id] = DLE.[DocumentLineId]
	WHERE DL.[State] = N'Reviewed'
	GROUP BY FE.[Index]
	HAVING SUM([Direction] * [Value]) <> 0;

	SELECT TOP (@Top) * FROM @ValidationErrors;