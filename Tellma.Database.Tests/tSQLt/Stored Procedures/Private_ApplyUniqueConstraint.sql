
CREATE PROCEDURE tSQLt.Private_ApplyUniqueConstraint 
  @ConstraintObjectId INT
AS
BEGIN
  DECLARE @SchemaName NVARCHAR(MAX);
  DECLARE @TableName NVARCHAR(MAX);
  DECLARE @ConstraintName NVARCHAR(MAX);
  DECLARE @CreateConstraintCmd NVARCHAR(MAX);
  DECLARE @AlterColumnsCmd NVARCHAR(MAX);
  DECLARE @NewNameOfOriginalConstraint NVARCHAR(MAX);
  DECLARE @QuotedFullConstraintName NVARCHAR(MAX);
  
  SELECT @SchemaName = SchemaName,
         @TableName = TableName,
         @ConstraintName = OBJECT_NAME(@ConstraintObjectId),
         @QuotedFullConstraintName = QUOTENAME(SchemaName)+'.'+QUOTENAME(OBJECT_NAME(@ConstraintObjectId))
    FROM tSQLt.Private_GetQuotedTableNameForConstraint(@ConstraintObjectId);
      
  SELECT @AlterColumnsCmd = NotNullColumnCmd,
         @CreateConstraintCmd = CreateConstraintCmd
    FROM tSQLt.Private_GetUniqueConstraintDefinition(@ConstraintObjectId, QUOTENAME(@SchemaName) + '.' + QUOTENAME(@TableName));

  EXEC tSQLt.Private_RenameObjectToUniqueName @SchemaName, @ConstraintName, @NewName = @NewNameOfOriginalConstraint OUTPUT;
  EXEC (@AlterColumnsCmd);
  EXEC (@CreateConstraintCmd);

  EXEC tSQLt.Private_MarktSQLtTempObject @ObjectName = @QuotedFullConstraintName, @ObjectType = 'CONSTRAINT', @NewNameOfOriginalObject = @NewNameOfOriginalConstraint;
END;
