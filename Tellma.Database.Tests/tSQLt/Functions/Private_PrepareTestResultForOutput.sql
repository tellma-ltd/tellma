CREATE FUNCTION tSQLt.Private_PrepareTestResultForOutput()
RETURNS TABLE
AS
RETURN
  SELECT ROW_NUMBER() OVER(ORDER BY Result DESC, Name ASC) No,Name [Test Case Name],
         RIGHT(SPACE(7)+CAST(DATEDIFF(MILLISECOND,TestStartTime,TestEndTime) AS VARCHAR(7)),7) AS [Dur(ms)], Result
    FROM tSQLt.TestResult;
