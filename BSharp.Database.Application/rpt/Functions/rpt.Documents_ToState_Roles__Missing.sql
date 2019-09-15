CREATE FUNCTION [rpt].[Documents_ToState_Roles__Missing] (
-- returns from the list of available roles the ones that are useful to move some docs @ToState
	@DocsIds dbo.IdList READONLY,
	@Roles dbo.IdList READONLY,
	@ToState NVARCHAR(30)
) RETURNS TABLE AS
RETURN
	SELECT T.Id AS DocumentId, T.RoleId FROM (
		SELECT D.[Id], WS.[RoleId]
		FROM dbo.Documents D
		JOIN dbo.Workflows W ON W.[DocumentTypeId] = D.[DocumentDefinitionId]
		JOIN dbo.[WorkflowSignatures] WS ON W.[Id] = WS.WorkflowId
		WHERE W.[ToState] = @ToState
		AND WS.Criteria IS NULL
		AND D.[Id] IN (SELECT [Id] FROM @DocsIds)
		AND WS.[RoleId] IN (SELECT [Id] FROM @Roles)
	) T
	LEFT JOIN dbo.DocumentSignatures DS ON T.Id = DS.DocumentId AND T.RoleId = DS.RoleId
	WHERE DS.RoleId IS NULL
GO;