CREATE PROCEDURE [rpt].[Tables__RowCounts]
AS
-- Credit: https://www.mssqltips.com/sqlservertip/2537/sql-server-row-count-for-all-tables-in-a-database/
    SELECT
          QUOTENAME(SCHEMA_NAME(sOBJ.schema_id)) + '.' + QUOTENAME(sOBJ.name) AS [TableName]
          , SUM(sPTN.rows) AS [RowCount]
    FROM 
          sys.objects AS sOBJ
          INNER JOIN sys.partitions AS sPTN
                ON sOBJ.object_id = sPTN.object_id
    WHERE
          sOBJ.type = 'U'
          AND sOBJ.is_ms_shipped = 0x0
          AND index_id < 2 -- 0:Heap, 1:Clustered
    GROUP BY 
          sOBJ.schema_id
          , sOBJ.name
    ORDER BY [TableName]
GO