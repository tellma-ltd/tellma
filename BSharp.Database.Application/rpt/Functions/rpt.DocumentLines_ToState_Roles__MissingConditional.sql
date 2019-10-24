CREATE FUNCTION [rpt].[DocumentLines_ToState_Roles__MissingConditional] (
-- returns from the list of available roles the ones that are useful to move some docs @ToState
	@DocLinesIds dbo.IdList READONLY,
	@Roles dbo.IdList READONLY,
	@ToState NVARCHAR(30)
) RETURNS TABLE AS
RETURN
	SELECT T.Id AS DocumentLineId, T.RoleId FROM (
		SELECT DL.[Id], DL.DocumentId, WS.[RoleId]
		FROM dbo.DocumentLines DL
		JOIN dbo.Workflows W ON W.[LineDefinitionId] = DL.[LineDefinitionId]
		JOIN dbo.[WorkflowSignatures] WS ON W.[Id] = WS.WorkflowId
		WHERE W.[ToState] = @ToState
		AND WS.Criteria IS NOT NULL
		AND bll.[fn_DocumentLine_Criteria__Satisfied](DL.[Id], WS.Criteria) = 1 -- signatures
		AND DL.[Id] IN (SELECT [Id] FROM @DocLinesIds)
		AND WS.[RoleId] IN (SELECT [Id] FROM @Roles)
	) T
	LEFT JOIN dbo.[DocumentLineSignatures] DS ON T.Id = DS.DocumentLineId AND T.RoleId = DS.RoleId
	WHERE DS.RoleId IS NULL
GO;