CREATE PROCEDURE tSQLt.InstallExternalAccessKey
AS
BEGIN
  EXEC tSQLt.Private_Print @Message='tSQLt.InstallExternalAccessKey is deprecated. Please use tSQLt.InstallAssemblyKey instead.';
  EXEC tSQLt.InstallAssemblyKey;
END;
