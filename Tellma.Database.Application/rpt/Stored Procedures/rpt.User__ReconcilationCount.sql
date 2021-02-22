CREATE PROCEDURE [rpt].[User__ReconcilationCount] -- [rpt].[User__ReconcilationCount] 45
@CreatedById INT = NULL
AS
	SELECT U.[Name], CONVERT(NVARCHAR(10), R.[CreatedAt], 102) AS [Date], Count(*) AS RecCount
	FROM reconciliations R
	JOIN dbo.Users U ON R.[CreatedById] = U.[Id]
	WHERE (@CreatedById IS NULL OR R.[CreatedById] = @CreatedById)
	GROUP BY ROLLUP(U.[Name], CONVERT(NVARCHAR(10), R.CreatedAt, 102));