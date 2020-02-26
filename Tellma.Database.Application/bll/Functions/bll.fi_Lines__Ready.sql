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
		SELECT [LineId], [RuleType], [RoleId]
		FROM map.RequiredSignatures(@Ids)
		WHERE ToState = @ToState
	),
	AvailableSignaturesForState AS (
		SELECT [LineId], [RuleType], [RoleId]
		FROM dbo.[LineSignatures]
		WHERE ToState = @ToState
		AND RevokedById IS NULL
	),
	LinesWithMissingSignaturesForState AS (
		SELECT [LineId], [RuleType], [RoleId]
		FROM RequiredSignaturesForState
		EXCEPT
		SELECT [LineId], [RuleType], [RoleId]
		FROM AvailableSignaturesForState
	)
	SELECT [Id]
	FROM @Ids
	EXCEPT
	SELECT LineId
	FROM LinesWithMissingSignaturesForState
);