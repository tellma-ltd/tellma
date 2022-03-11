
CREATE PROCEDURE tSQLt.Private_PrintXML
    @Message XML
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(@Message AS XML);--Required together with ":XML ON" sqlcmd statement to allow more than 1mb to be returned
    RETURN 0;
END;
