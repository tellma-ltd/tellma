CREATE FUNCTION [rpt].[DocumentLinesWithNoDefinedWorkflows] (
	@DocumentLinesIds dbo.[IdList] READONLY
) RETURNS TABLE AS
RETURN
(
	SELECT [Id] AS DocumentLineId
	FROM dbo.DocumentLines
	WHERE [Id] IN (SELECT [Id] FROM @DocumentLinesIds)
	AND [DefinitionId] NOT IN (
		SELECT [LineDefinitionId] FROM dbo.Workflows
	)
)