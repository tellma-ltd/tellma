CREATE FUNCTION [bll].[fi_Lines__Ready]
(
	-- Determine which of the selected Lines are ready for state change
	-- Note that If a line definition does not a have a workflow, the transition is always accepted
	@Ids dbo.IdList READONLY,
	@ToState SMALLINT,
	@LinesSatisfyingCriteria IdWithCriteriaList READONLY
)
RETURNS TABLE AS RETURN
(
	--WITH RequiredSignatures AS (
	--	-- Signatures always required
	--	SELECT L.Id AS LineId, WS.RoleId, W.ToState
	--	FROM dbo.[Lines] L
	--	JOIN dbo.Workflows W ON L.[DefinitionId] = W.LineDefinitionId AND L.[State] = W.[FromState]
	--	JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
	--	WHERE L.[Id] IN (SELECT [Id] FROM @Ids)
	--	AND (WS.[Criteria] IS NULL)
	--	UNION
	--	-- Signatures required because criteria was satisfied
	--	SELECT L.Id AS LineId, WS.RoleId, W.ToState
	--	FROM dbo.[Lines] L
	--	JOIN dbo.Workflows W ON L.[DefinitionId] = W.LineDefinitionId AND L.[State] = W.[FromState]
	--	JOIN dbo.WorkflowSignatures WS ON W.[Id] = WS.[WorkflowId]
	--	JOIN @LinesCriteria LC ON L.[Id] = LC.[Id] AND WS.[Criteria] = LC.[Criteria]
	--	WHERE L.[Id] IN (SELECT [Id] FROM @Ids)
	--	AND (WS.[Criteria] IS NOT NULL)
	--),
	WITH RequiredSignaturesForState AS (
		SELECT [LineId], RoleId
		FROM map.RequiredSignatures(@Ids, @LinesSatisfyingCriteria)
		WHERE ToState = @ToState
	),
	AvailableSignaturesForState AS (
		SELECT [LineId], RoleId
		FROM dbo.[LineSignatures]
		WHERE ToState = @ToState
		AND RevokedById IS NULL
	),
	LinesWithMissingSignaturesForState AS (
		SELECT LineId, RoleId
		FROM RequiredSignaturesForState
		EXCEPT
		SELECT [LineId], RoleId
		FROM AvailableSignaturesForState
	)
	SELECT [Id]
	FROM @Ids
	EXCEPT
	SELECT LineId
	FROM LinesWithMissingSignaturesForState
)
