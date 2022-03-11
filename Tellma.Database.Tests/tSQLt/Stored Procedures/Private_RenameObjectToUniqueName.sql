
CREATE PROCEDURE tSQLt.Private_RenameObjectToUniqueName
    @SchemaName NVARCHAR(MAX),
    @ObjectName NVARCHAR(MAX),
    @NewName NVARCHAR(MAX) = NULL OUTPUT
AS
BEGIN
   SET @NewName=ISNULL(@NewName, tSQLt.Private::CreateUniqueObjectName());
   
   EXEC tSQLt.Private_MarkObjectBeforeRename @SchemaName, @ObjectName;

   EXEC tSQLt.Private_RenameObject @SchemaName,
                                   @ObjectName,
                                   @NewName;

END;


