CREATE FUNCTION [bll].[fi_Lines__Ready]
(
	-- Determine which of the selected Lines are ready for state change
	-- Note that If a line definition does not a have a workflow, the transition is always accepted
	@Ids dbo.IdList READONLY,
	@ToState SMALLINT
)
RETURNS TABLE AS RETURN
(
	WITH RequiredSignaturesForState AS (
		SELECT [LineId], [RuleType],
		IIF([RuleType] = N'ByRole',[RoleId], -1) AS [RoleId],
		IIF([RuleType] IN (N'ByUser', N'ByAgent'), [UserId], -1) AS [UserId]
		FROM map.[LinesRequiredSignatures](@Ids)
		WHERE ToState = @ToState
	),
	AvailableSignaturesForState AS (
		SELECT [LineId], [RuleType],
		IIF([RuleType] = N'ByRole',[RoleId], -1) AS [RoleId],
		IIF([RuleType] IN (N'ByUser', N'ByAgent'), [OnbehalfofUserid], -1) AS [UserId]		
		FROM dbo.[LineSignatures]
		WHERE ToState = @ToState
		AND [LineId] IN (SELECT [Id] FROM @Ids)
		AND RevokedById IS NULL
	),
	LinesWithMissingSignaturesForState AS (
		SELECT [LineId], [RuleType], [RoleId], [UserId]
		FROM RequiredSignaturesForState
		EXCEPT
		SELECT [LineId], [RuleType], [RoleId], [UserId]
		FROM AvailableSignaturesForState
	)
	SELECT [Id]
	FROM @Ids
	EXCEPT
	SELECT LineId
	FROM LinesWithMissingSignaturesForState
);