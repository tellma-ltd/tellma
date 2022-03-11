CREATE PROCEDURE tSQLt.Private_EnableCLR
AS
BEGIN
  EXEC master.sys.sp_configure @configname='clr enabled', @configvalue = 1;
  RECONFIGURE;
END;
