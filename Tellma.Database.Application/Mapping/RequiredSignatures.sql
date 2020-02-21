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
		SELECT L.Id As LineId, WS.RoleId, W.ToState, WS.ProxyRoleId
		FROM dbo.Lines L
		JOIN dbo.Workflows W ON W.LineDefinitionId = L.DefinitionId
		JOIN dbo.WorkflowSignatures WS ON WS.WorkflowId = W.[Id]
		WHERE L.Id IN (SELECT [Id] FROM @LineIds) AND (WS.[Criteria] IS NULL)
		UNION
		-- Signatures required because criteria was satisfied
		SELECT L.Id As LineId, WS.RoleId, W.ToState, WS.ProxyRoleId
		FROM dbo.Lines L
		JOIN dbo.Workflows W ON W.LineDefinitionId = L.DefinitionId
		JOIN dbo.WorkflowSignatures WS ON WS.WorkflowId = W.[Id]
		JOIN @LinesSatisfyingCriteria LC ON L.[Id] = LC.[Id] AND WS.[Criteria] = LC.[Criteria]
		WHERE L.Id IN (SELECT [Id] FROM @LineIds) AND (WS.[Criteria] IS NOT NULL)
	)
	SELECT RS.[LineId], RS.[ToState], RS.RoleId, LS.CreatedById AS SignedById, LS.CreatedAt AS SignedAt, LS.OnBehalfOfUserId,
		IIF(RM.RoleId IS NULL, 0, 1) AS CanSign, RS.ProxyRoleId, IIF(RM2.RoleId IS NULL, 0, 1) AS CanSignOnBehalf
	FROM ApplicableSignatures RS
	LEFT JOIN dbo.LineSignatures LS ON RS.[LineId] = LS.LineId AND RS.RoleId = LS.RoleId AND RS.ToState = LS.ToState AND LS.RevokedAt IS NOT NULL
	LEFT JOIN (
		SELECT RoleId FROM dbo.RoleMemberships
		WHERE UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId'))
	) RM ON LS.RoleId = RM.RoleId
	LEFT JOIN (
		SELECT RoleId FROM dbo.RoleMemberships
		WHERE UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId'))
	) RM2 ON RS.ProxyRoleId = RM.RoleId
-- AND RuleType = N'HasRole'
	-- UNION
);