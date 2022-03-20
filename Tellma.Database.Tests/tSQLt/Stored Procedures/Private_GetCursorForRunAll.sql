
CREATE PROCEDURE tSQLt.Private_GetCursorForRunAll
AS
BEGIN
  INSERT INTO #TestClassesForRunCursor
   SELECT Name
     FROM tSQLt.TestClasses;
END;
