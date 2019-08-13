CREATE FUNCTION [rpt].[DocumentsWithNoDefinedWorkflows] (
	@Ids dbo.[IdList] READONLY
) RETURNS TABLE AS
RETURN
(
	SELECT [Id]
	FROM dbo.Documents
	WHERE [Id] IN (SELECT [Id] FROM @Ids)
	AND [DocumentTypeId] NOT IN (
		SELECT [DocumentTypeId] FROM dbo.Workflows
	)
)