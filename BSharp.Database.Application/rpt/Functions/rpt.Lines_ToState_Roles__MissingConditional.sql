CREATE FUNCTION [rpt].[Lines_ToState_Roles__MissingConditional] (
-- returns from the list of available roles the ones that are useful to move some docs @ToState
	@DocLinesIds dbo.IdList READONLY,
	@Roles dbo.IdList READONLY,
	@ToState NVARCHAR(30)
) RETURNS TABLE AS
RETURN
	SELECT T.Id AS LineId, T.RoleId FROM (
		SELECT DL.[Id], DL.[DocumentId], WS.[RoleId]
		FROM dbo.[Lines] DL
		JOIN dbo.Workflows W ON W.[LineDefinitionId] = DL.[DefinitionId]
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
		AND WS.Criteria IS NOT NULL
		AND bll.[fn_Line_Criteria__Satisfied](DL.[Id], WS.Criteria) = 1 -- signatures
		AND DL.[Id] IN (SELECT [Id] FROM @DocLinesIds)
		AND WS.[RoleId] IN (SELECT [Id] FROM @Roles)
	) T
	LEFT JOIN dbo.[LineSignatures] DS ON T.Id = DS.[LineId] AND T.RoleId = DS.RoleId
	WHERE DS.RoleId IS NULL
GO;