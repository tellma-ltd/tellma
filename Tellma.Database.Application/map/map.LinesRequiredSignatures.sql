CREATE FUNCTION [map].[LinesRequiredSignatures] (
	@LineIds IdList READONLY
)
RETURNS TABLE
AS
RETURN (
	WITH ApplicableSignatures AS
	(
		SELECT L.Id As LineId, WS.RuleType, WS.[RuleTypeEntryIndex], 
			WS.RoleId,
			COALESCE(
				WS.UserId,
				(
					SELECT MIN(UserId) FROM dbo.[RelationUsers] WHERE [RelationId] IN (
						SELECT [Relation1Id] FROM dbo.[Relations] WHERE [Id] IN (
							SELECT [RelationId] FROM dbo.Entries WHERE LineId = L.Id AND [Index] = WS.[RuleTypeEntryIndex]
						)
					)
				)
			) AS UserId,
			(
				SELECT MIN(UserId) FROM dbo.RoleMemberships
				WHERE [RoleId] = WS.[RoleId]
				GROUP BY [RoleId]
				HAVING MIN([UserId]) = MAX([UserId])
			) AS OnBehalfOfRoleUserId,
			(
				SELECT RL.[Relation1Id] FROM dbo.Entries E
				JOIN dbo.[Relations] RL ON E.[RelationId] = RL.[Id]
				WHERE LineId = L.Id
				AND [Index] = WS.[RuleTypeEntryIndex]
			) AS [CustodianId],
			WS.PredicateType, WS.[PredicateTypeEntryIndex], WS.[Value],
			W.ToState, WS.[ProxyRoleId]
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
					WHERE [Index] = WS.[PredicateTypeEntryIndex]
					AND [Value] >= WS.[Value]
				)
			)
		)
	),
	AvailableSignatures AS
	(
		SELECT
			RS.[LineId], LS.Id AS LineSignatureId,
			COALESCE(LS.[ToState], RS.[ToState]) AS ToState,
			RS.RuleType, RS.RoleId, RS.UserId, RS.[CustodianId],
			LS.CreatedById AS SignedById, LS.CreatedAt AS SignedAt,
			COALESCE(LS.[OnBehalfOfUserId], RS.[OnBehalfOfRoleUserId]) AS OnBehalfOfUserId,
			CAST(IIF(RM.RoleId IS NULL, 0, 1) AS BIT) AS CanSign,
			RS.ProxyRoleId,
			CAST(IIF(RM2.RoleId IS NULL, 0, 1) AS BIT) AS CanSignOnBehalf,
			LS.ReasonId, LS.ReasonDetails
		FROM ApplicableSignatures RS
		LEFT JOIN dbo.LineSignatures LS ON
			RS.[RoleId] = LS.[RoleId] AND
			RS.[LineId] = LS.LineId AND RS.RuleType = LS.RuleType AND RS.ToState = ABS(LS.ToState) AND LS.RevokedAt IS NULL
		LEFT JOIN (
			SELECT RoleId FROM dbo.RoleMemberships
			WHERE UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId'))
		) RM ON RS.RoleId = RM.RoleId
		LEFT JOIN (
			SELECT RoleId FROM dbo.RoleMemberships
			WHERE UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId'))
		) RM2 ON RS.ProxyRoleId = RM2.RoleId
		WHERE RS.RuleType = N'ByRole'
		UNION
		SELECT
			RS.[LineId], LS.Id AS LineSignatureId,
			COALESCE(LS.[ToState], RS.[ToState]) AS ToState,
			RS.RuleType, RS.RoleId, RS.UserId, RS.[CustodianId],
			LS.CreatedById AS SignedById, LS.CreatedAt AS SignedAt,
			COALESCE(LS.[OnBehalfOfUserId], RS.[UserId]) AS OnBehalfOfUserId,
			CAST(IIF(RS.UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId')), 1, 0) AS BIT) AS CanSign,
			RS.ProxyRoleId,
			CAST(IIF(RM.RoleId IS NULL, 0, 1) AS BIT) AS CanSignOnBehalf,
			LS.ReasonId, LS.ReasonDetails
		FROM ApplicableSignatures RS
		LEFT JOIN dbo.LineSignatures LS ON
			-- We need to test what happens if two users are required to sign, 
			-- or two custodians are required to sign
			-- or one user and one custodian
			RS.[LineId] = LS.LineId AND RS.RuleType = LS.RuleType AND RS.UserId = LS.OnBehalfOfUserId AND RS.ToState = ABS(LS.ToState) AND LS.RevokedAt IS NULL
		LEFT JOIN (
			SELECT RoleId FROM dbo.RoleMemberships
			WHERE UserId = CONVERT(INT, SESSION_CONTEXT(N'UserId'))
		) RM ON RS.ProxyRoleId = RM.RoleId
		WHERE RS.RuleType IN (N'ByUser', N'ByCustodian')
		UNION
		SELECT
			RS.[LineId], LS.Id AS LineSignatureId,
			COALESCE(LS.[ToState], RS.[ToState]) AS ToState,
			RS.RuleType, RS.RoleId, RS.UserId, RS.[CustodianId],
			LS.CreatedById AS SignedById, LS.CreatedAt AS SignedAt,
			COALESCE(LS.[OnBehalfOfUserId], RS.[UserId]) AS OnBehalfOfUserId,
			CAST(1 AS BIT) AS CanSign,
			RS.ProxyRoleId,
			CAST(1 AS BIT) AS CanSignOnBehalf,
			LS.ReasonId, LS.ReasonDetails
			FROM ApplicableSignatures RS
			LEFT JOIN dbo.LineSignatures LS ON
				RS.[LineId] = LS.LineId AND RS.RuleType = LS.RuleType AND RS.ToState = ABS(LS.ToState) AND LS.RevokedAt IS NULL
		WHERE RS.RuleType = N'Public'
	)
	SELECT
		LineId, LineSignatureId, ToState, RuleType, RoleId, UserId, [CustodianId],
		SignedById, SignedAt, OnBehalfOfUserId,
		(SELECT MIN(ToState) FROM AvailableSignatures
		WHERE LineId = S.LineId AND ToState < S.ToState AND ToState > 0
		AND SignedById IS NULL) AS LastUnsignedState,
		-(SELECT MAX(ABS(ToState)) FROM AvailableSignatures
		WHERE LineId = S.LineId AND ToState < 0
		AND SignedById IS NOT NULL) AS LastNegativeState,
		CanSign, ProxyRoleId, CanSignOnBehalf, ReasonId, ReasonDetails
	FROM AvailableSignatures S
);