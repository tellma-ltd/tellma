CREATE FUNCTION [rpt].[DocumentLines_ToState_Roles__Missing] (
-- returns from the list of available roles the ones that are useful to move some doclines to @ToState
	@DocLinesIds dbo.IdList READONLY,
	@Roles dbo.IdList READONLY,
	@ToState NVARCHAR(30)
) RETURNS TABLE AS
RETURN
	SELECT T.Id AS DocumentLineId, T.RoleId FROM (
		SELECT DL.[Id], DL.[DocumentId], WS.[RoleId]
		FROM dbo.DocumentLines DL
		JOIN dbo.Workflows W ON W.[LineDefinitionId] = DL.[LineDefinitionId]
		JOIN dbo.[WorkflowSignatures] WS ON W.[Id] = WS.WorkflowId
		-- Workflows are defined for positive states. Hence when moving to negative state we need to use ABS value
		WHERE W.[ToState] = CASE
			WHEN @ToState = N'Rejected'	THEN N'Authorized'
			WHEN @ToState = N'Failed'	THEN N'Completed'
			WHEN @ToState = N'Invalid'	THEN N'Reviewed'
			ELSE @ToState
		END
		-- IF FromState is negative we need to unsign the line first
		AND W.[FromState] = DL.[State]
		AND WS.[Criteria] IS NULL
		AND DL.[Id] IN (SELECT [Id] FROM @DocLinesIds)
		AND WS.[RoleId] IN (SELECT [Id] FROM @Roles)
	) T
	LEFT JOIN dbo.[DocumentLineSignatures] DS ON T.Id = DS.DocumentLineId AND T.RoleId = DS.RoleId
	WHERE DS.RoleId IS NULL
GO;