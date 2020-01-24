CREATE FUNCTION [rpt].[LinesWithNoDefinedWorkflows] (
	@LinesIds dbo.[IdList] READONLY
) RETURNS TABLE AS
RETURN
(
	SELECT [Id] AS LineId
	FROM dbo.[Lines]
	WHERE [Id] IN (SELECT [Id] FROM @LinesIds)
	AND [DefinitionId] NOT IN (
		SELECT [LineDefinitionId] FROM dbo.Workflows
	)
)