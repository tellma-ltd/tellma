CREATE VIEW [dbo].[WorkflowsView]
-- TODO: Can be made into materialized view to add index on [FromState] and to improve performance
AS
SELECT 
	[Id],
	[LineDefinitionId], 
	(
		SELECT ISNULL(MAX([ToState]),0)
		FROM dbo.Workflows
		WHERE [LineDefinitionId] = W.[LineDefinitionId] AND [TOState] < W.[ToState]
	) AS [FromState],
	[ToState],
	[SavedById]
FROM dbo.Workflows W
