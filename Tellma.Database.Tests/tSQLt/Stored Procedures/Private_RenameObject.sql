
CREATE PROCEDURE tSQLt.Private_RenameObject
    @SchemaName NVARCHAR(MAX),
    @ObjectName NVARCHAR(MAX),
    @NewName NVARCHAR(MAX)
AS
BEGIN
   DECLARE @RenameCmd NVARCHAR(MAX);
   SET @RenameCmd = 'EXEC sp_rename ''' + 
                    REPLACE(@SchemaName + '.' + @ObjectName, '''', '''''') + ''', ''' + 
                    REPLACE(@NewName, '''', '''''') + ''',''OBJECT'';';
   
   EXEC tSQLt.SuppressOutput @RenameCmd;
END;


