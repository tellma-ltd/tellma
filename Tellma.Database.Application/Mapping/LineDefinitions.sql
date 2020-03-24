CREATE FUNCTION [map].[LineDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT LD.*,
		IIF(W.[LineDefinitionId] IS NULL, 0, 1) AS HasWorkflow
	FROM [dbo].[LineDefinitions] LD
	LEFT JOIN (
		Select DISTINCT LineDefinitionId
		FROM dbo.Workflows
	) W ON W.LineDefinitionId = LD.[Id]
);
