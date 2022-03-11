
CREATE PROCEDURE tSQLt.Private_RunTestClass
  @TestClassName NVARCHAR(MAX)
AS
BEGIN
    DECLARE @TestCaseName NVARCHAR(MAX);
    DECLARE @TestClassId INT; SET @TestClassId = tSQLt.Private_GetSchemaId(@TestClassName);
    DECLARE @SetupProcName NVARCHAR(MAX);
    DECLARE @CleanUpProcName NVARCHAR(MAX);
    EXEC tSQLt.Private_GetClassHelperProcedureName @TestClassId, @SetupProcName OUT, @CleanUpProcName OUT;
    
    DECLARE @cmd NVARCHAR(MAX) = (
      (
        SELECT 'EXEC tSQLt.Private_RunTest '''+REPLACE(tSQLt.Private_GetQuotedFullName(object_id),'''','''''')+''', '+ISNULL(''''+REPLACE(@SetupProcName,'''','''''')+'''','NULL')+', '+ISNULL(''''+REPLACE(@CleanUpProcName,'''','''''')+'''','NULL')+';'
          FROM sys.procedures
         WHERE schema_id = @TestClassId
           AND LOWER(name) LIKE 'test%'
         ORDER BY NEWID()
           FOR XML PATH(''),TYPE
      ).value('.','NVARCHAR(MAX)')
    );
    EXEC(@cmd);
END;
