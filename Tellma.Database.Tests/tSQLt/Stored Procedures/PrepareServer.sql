CREATE PROCEDURE tSQLt.PrepareServer
AS
BEGIN
  EXEC tSQLt.Private_EnableCLR;
  EXEC tSQLt.InstallAssemblyKey;
END;
