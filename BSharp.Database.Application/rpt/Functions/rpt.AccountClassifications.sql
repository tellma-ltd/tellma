CREATE FUNCTION [rpt].[AccountClassifications] ()
RETURNS TABLE AS 
RETURN (
	SELECT
		[Id],
		--(SELECT [Id] FROM dbo.GLAccounts WHERE [Code] = (
		--	(SELECT MAX([Code]) FROM dbo.GLAccounts WHERE GLA.[Code] LIKE [Code] + '%' AND GLA.[Code] <> [Code])
		--)) AS ParentId,
		(SELECT [Id] FROM dbo.[AccountClassifications] WHERE [Node] = GLA.[ParentNode]) AS ParentId,
		[Name], [Name2], [Name3], [Code]
	FROM dbo.[AccountClassifications] GLA
);