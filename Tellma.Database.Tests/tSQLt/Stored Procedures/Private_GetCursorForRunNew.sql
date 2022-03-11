
CREATE PROCEDURE tSQLt.Private_GetCursorForRunNew
AS
BEGIN
  INSERT INTO #TestClassesForRunCursor
   SELECT TC.Name
     FROM tSQLt.TestClasses AS TC
     JOIN tSQLt.Private_NewTestClassList AS PNTCL
       ON PNTCL.ClassName = TC.Name;
END;
