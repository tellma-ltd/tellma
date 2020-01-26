CREATE FUNCTION [bll].[fi_Lines__Ready]
(
	-- Determine which of the selected Lines are ready for state change
	-- Note that If a line definition does not a have a workflow, the transition is always accepted
	@Ids dbo.IdList READONLY,
	@ToState SMALLINT -- NVARCHAR (30)
)
RETURNS TABLE AS RETURN
(
	WITH RequiredSignatures AS (
		SELECT DL.Id AS LineId, WS.RoleId
		FROM @Ids FE
		JOIN dbo.[Lines] DL ON FE.[Id] = DL.[Id]
		JOIN dbo.Workflows W ON DL.[DefinitionId] = W.LineDefinitionId AND DL.[State] = W.[FromState]
		JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
		WHERE [bll].[fn_Line_Criteria__Satisfied](FE.[Id], WS.Criteria) = 1	
	),
	AvailableSignatures AS (
		SELECT [LineId], RoleId
		FROM dbo.[LineSignatures]
		WHERE ToState = @ToState
		AND RevokedById IS NULL
	),
	LinesWithMissingSignatures AS (
		SELECT LineId, RoleId
		FROM RequiredSignatures
		EXCEPT
		SELECT [LineId], RoleId
		FROM AvailableSignatures
	)
	SELECT [Id]
	FROM @Ids
	EXCEPT
	SELECT LineId
	FROM LinesWithMissingSignatures
)
