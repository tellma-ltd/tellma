CREATE FUNCTION [bll].[fi_ReadyDocumentLines]
(
	-- Determine which of the selected Lines are reacdy for state change
	-- Note that If a line definition does not a have a workflow, the transition is always accepted
	@Ids dbo.IdList READONLY,
	@ToState NVARCHAR (30)
)
RETURNS TABLE AS RETURN
(
	WITH RequiredSignatures AS (
		SELECT DL.Id AS DocumentLineId, WS.RoleId
		FROM @Ids FE
		JOIN dbo.DocumentLines DL ON FE.[Id] = DL.[Id]
		JOIN dbo.Workflows W ON DL.LineDefinitionId = W.LineDefinitionId AND DL.[State] = W.[FromState]
		JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
		WHERE [bll].[fn_DocumentLine_Criteria__Satisfied](FE.[Id], WS.Criteria) = 1	
	),
	AvailabeSignatures AS (
		SELECT DocumentLineId, RoleId
		FROM dbo.DocumentLineSignatures
		WHERE ToState = @ToState
		AND RevokedById IS NULL
	),
	LinesWithMissingSignatures AS (
		SELECT DocumentLineId, RoleId
		FROM RequiredSignatures
		EXCEPT
		SELECT DocumentLineId, RoleId
		FROM AvailabeSignatures
	)
	SELECT [Id]
	FROM @Ids
	EXCEPT
	SELECT DocumentLineId
	FROM LinesWithMissingSignatures
)
