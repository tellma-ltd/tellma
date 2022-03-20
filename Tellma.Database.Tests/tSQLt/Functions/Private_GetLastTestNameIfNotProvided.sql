
----------------------------------------------------------------------
CREATE FUNCTION tSQLt.Private_GetLastTestNameIfNotProvided(@TestName NVARCHAR(MAX))
RETURNS TABLE
AS
RETURN
  SELECT CASE WHEN (LTRIM(ISNULL(@TestName,'')) = '') THEN LE.TestName ELSE @TestName END TestName
    FROM tSQLt.Run_LastExecution LE
    RIGHT JOIN sys.dm_exec_sessions ES
      ON LE.SessionId = ES.session_id
      AND LE.LoginTime = ES.login_time
    WHERE ES.session_id = @@SPID;
