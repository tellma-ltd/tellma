CREATE PROCEDURE [rpt].[User__ReconcilationCount] -- [rpt].[User__ReconcilationCount] 45
@CreatedById INT
AS
	SELECT  CONVERT(NVARCHAR(10), CreatedAt, 102) AS [Date], Count(*) AS RecCount
	FROM reconciliations
	WHERE CreatedById = @CreatedById
	GROUP BY ROLLUP(CONVERT(NVARCHAR(10), CreatedAt, 102))
--	ORDER BY CONVERT(NVARCHAR(10), CreatedAt, 102)