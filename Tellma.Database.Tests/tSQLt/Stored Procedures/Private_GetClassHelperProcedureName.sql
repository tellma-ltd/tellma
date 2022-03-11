

CREATE PROCEDURE tSQLt.Private_GetClassHelperProcedureName
  @TestClassId INT = NULL,
  @SetupProcName NVARCHAR(MAX) OUTPUT,
  @CleanUpProcName NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SELECT @SetupProcName = tSQLt.Private_GetQuotedFullName(object_id)
      FROM sys.procedures
     WHERE schema_id = @TestClassId
       AND LOWER(name) = 'setup';
    SELECT @CleanUpProcName = tSQLt.Private_GetQuotedFullName(object_id)
      FROM sys.procedures
     WHERE schema_id = @TestClassId
       AND LOWER(name) = 'cleanup';
END;
