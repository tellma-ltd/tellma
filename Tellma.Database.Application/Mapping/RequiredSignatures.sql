CREATE FUNCTION [map].[RequiredSignatures] (
	@LineIds IdList READONLY
)
RETURNS TABLE
AS
RETURN (
	WITH ApplicableSignatures AS
	(
		SELECT L.Id As LineId, WS.RuleType, WS.[RuleTypeEntryNumber],  WS.RoleId, COALESCE(
				WS.UserId,
				(SELECT UserId FROM dbo.Agents WHERE AgentId IN (
					SELECT AgentId FROM dbo.Entries WHERE LineId = L.Id AND EntryNumber = WS.[RuleTypeEntryNumber]
					)
				)
			) AS UserId,
			WS.PredicateType, WS.[PredicateTypeEntryNumber], WS.[Value], W.ToState, WS.ProxyRoleId
		FROM dbo.Lines L
		JOIN dbo.Workflows W ON W.LineDefinitionId = L.DefinitionId
		JOIN dbo.WorkflowSignatures WS ON WS.WorkflowId = W.[Id]
		WHERE L.Id IN (SELECT [Id] FROM @LineIds)
		AND (
			WS.[PredicateType] IS NULL
			OR (
				WS.[PredicateType] = N'ValueGreaterOrEqual'
				AND L.[Id] IN (
					SELECT LineId FROM dbo.Entries
					WHERE EntryNumber = WS.[PredicateTypeEntryNumber]
					AND [Value] >= WS.[Value]
				)
			)
		)
	),
	AvailableSignatures AS
	(
		SELECT
			RS.[LineId],
			COALESCE(LS.[ToState], RS.[ToState]) AS ToState,
			RS.RuleType, RS.RoleId, RS.UserId,
			LS.CreatedById AS SignedById, LS.CreatedAt AS SignedAt, LS.OnBehalfOfUserId,
			CAST(IIF(RM.RoleId IS NULL, 0, 1) AS BIT) AS CanSign,
			RS.ProxyRoleId,
			CAST(IIF(RM2.RoleId IS NULL, 0, 1) AS BIT) AS CanSignOnBehalf,
			LS.ReasonId, LS.ReasonDetails
		FROM ApplicableSignatures RS
		LEFT JOIN dbo.LineSignatures LS ON RS.[LineId] = LS.LineId AND RS.RuleType = LS.RuleType AND RS.ToState = ABS(LS.ToState) AND LS.RevokedAt IS NULL
		LEFT JOIN (
			SELECT RoleId FROM dbo.RoleMemberships
			WHERE UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId'))
		) RM ON RS.RoleId = RM.RoleId
		LEFT JOIN (
			SELECT RoleId FROM dbo.RoleMemberships
			WHERE UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId'))
		) RM2 ON RS.ProxyRoleId = RM.RoleId
		WHERE RS.RuleType = N'ByRole'
		UNION
		SELECT RS.[LineId], COALESCE(LS.[ToState], RS.[ToState]) AS ToState, RS.RuleType, RS.RoleId, RS.UserId, LS.CreatedById AS SignedById, LS.CreatedAt AS SignedAt, LS.OnBehalfOfUserId,
			CAST(IIF(RS.UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId')), 1, 0) AS BIT) AS CanSign,
			RS.ProxyRoleId,
			CAST(IIF(RM.RoleId IS NULL, 0, 1) AS BIT) AS CanSignOnBehalf,
			LS.ReasonId, LS.ReasonDetails
		FROM ApplicableSignatures RS
		LEFT JOIN dbo.LineSignatures LS ON RS.[LineId] = LS.LineId AND RS.RuleType = LS.RuleType AND RS.UserId = LS.OnBehalfOfUserId AND RS.ToState = ABS(LS.ToState) AND LS.RevokedAt IS NULL
		LEFT JOIN (
			SELECT RoleId FROM dbo.RoleMemberships
			WHERE UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId'))
		) RM ON RS.ProxyRoleId = RM.RoleId
		WHERE RS.RuleType IN(N'ByUser', N'ByAgent')
		UNION
		SELECT
			RS.[LineId], COALESCE(LS.[ToState], RS.[ToState]) AS ToState, RS.RuleType, RS.RoleId, RS.UserId, LS.CreatedById AS SignedById, LS.CreatedAt AS SignedAt, LS.OnBehalfOfUserId,
			CAST(1 AS BIT) AS CanSign,
			RS.ProxyRoleId,
			CAST(1 AS BIT) AS CanSignOnBehalf,
			LS.ReasonId, LS.ReasonDetails
			FROM ApplicableSignatures RS
			LEFT JOIN dbo.LineSignatures LS ON RS.[LineId] = LS.LineId AND RS.RuleType = LS.RuleType AND RS.ToState = ABS(LS.ToState) AND LS.RevokedAt IS NULL
		WHERE RS.RuleType = N'Public'
	)
	SELECT
		LineId, ToState, RuleType, RoleId, UserId, SignedById, SignedAt, OnBehalfOfUserId,
		(SELECT MIN(ToState) FROM AvailableSignatures WHERE LineId = S.LineId AND ToState < S.ToState AND ToState > 0 AND SignedById IS NULL) AS LastUnsignedState,
		-(SELECT MAX(ABS(ToState)) FROM AvailableSignatures WHERE LineId = S.LineId AND ABS(ToState) < ABS(S.ToState) AND ToState < 0 AND SignedById IS NULL) AS LastNegativeState,
		CanSign, ProxyRoleId, CanSignOnBehalf, ReasonId, ReasonDetails
	FROM AvailableSignatures S
);