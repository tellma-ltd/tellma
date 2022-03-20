

CREATE PROCEDURE tSQLt.Private_RunCursor
  @TestResultFormatter NVARCHAR(MAX),
  @GetCursorCallback NVARCHAR(MAX)
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @TestClassName NVARCHAR(MAX);
  DECLARE @TestProcName NVARCHAR(MAX);

  CREATE TABLE #TestClassesForRunCursor(Name NVARCHAR(MAX));
  EXEC @GetCursorCallback;
----  
  DECLARE @cmd NVARCHAR(MAX) = (
    (
      SELECT 'EXEC tSQLt.Private_RunTestClass '''+REPLACE(Name, '''' ,'''''')+''';'
        FROM #TestClassesForRunCursor
         FOR XML PATH(''),TYPE
    ).value('.','NVARCHAR(MAX)')
  );
  EXEC(@cmd);
  
  EXEC tSQLt.Private_OutputTestResults @TestResultFormatter;
END;
