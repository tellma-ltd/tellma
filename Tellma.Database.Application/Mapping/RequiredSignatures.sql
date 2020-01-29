CREATE FUNCTION [map].[RequiredSignatures] (
	@LineIds IdList READONLY,
	@LinesSatisfyingCriteria IdWithCriteriaList READONLY
)
RETURNS TABLE
AS
RETURN (
	WITH ApplicableSignatures AS
	(
		-- Signatures always required
		SELECT L.Id As LineId, WS.RoleId, W.ToState
		FROM dbo.Lines L
		JOIN dbo.Workflows W ON W.LineDefinitionId = L.DefinitionId
		JOIN dbo.WorkflowSignatures WS ON WS.WorkflowId = W.[Id]
		WHERE L.Id IN (SELECT [Id] FROM @LineIds) AND (WS.[Criteria] IS NULL)
		UNION
		-- Signatures required because criteria was satisfied
		SELECT L.Id As LineId, WS.RoleId, W.ToState
		FROM dbo.Lines L
		JOIN dbo.Workflows W ON W.LineDefinitionId = L.DefinitionId
		JOIN dbo.WorkflowSignatures WS ON WS.WorkflowId = W.[Id]
		JOIN @LinesSatisfyingCriteria LC ON L.[Id] = LC.[Id] AND WS.[Criteria] = LC.[Criteria]
		WHERE L.Id IN (SELECT [Id] FROM @LineIds) AND (WS.[Criteria] IS NOT NULL)
	)
	SELECT RS.[LineId], RS.[ToState], RS.RoleId, LS.CreatedById AS SignedById
	FROM ApplicableSignatures RS
	LEFT JOIN dbo.LineSignatures LS ON RS.[LineId] = LS.LineId AND RS.RoleId = LS.RoleId AND RS.ToState = LS.ToState AND LS.RevokedAt IS NOT NULL
-- AND RuleType = N'HasRole'
	-- UNION
);