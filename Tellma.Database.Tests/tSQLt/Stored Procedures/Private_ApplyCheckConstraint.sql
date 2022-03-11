
CREATE PROCEDURE tSQLt.Private_ApplyCheckConstraint
  @ConstraintObjectId INT
AS
BEGIN
  DECLARE @Cmd NVARCHAR(MAX);
  DECLARE @NewNameOfOriginalConstraint NVARCHAR(MAX);
  DECLARE @QuotedFullConstraintName NVARCHAR(MAX);
  SELECT @Cmd = 'CONSTRAINT ' + QUOTENAME(name) + ' CHECK' + definition 
    FROM sys.check_constraints
   WHERE object_id = @ConstraintObjectId;
  
  DECLARE @QuotedTableName NVARCHAR(MAX);
  
  SELECT @QuotedTableName = QuotedTableName FROM tSQLt.Private_GetQuotedTableNameForConstraint(@ConstraintObjectId);

  SELECT @Cmd = 'ALTER TABLE ' + @QuotedTableName + ' ADD ' + @Cmd,
         @QuotedFullConstraintName = QUOTENAME(SCHEMA_NAME(schema_id))+'.'+QUOTENAME(name)
    FROM sys.objects 
   WHERE object_id = @ConstraintObjectId;

  EXEC tSQLt.Private_RenameObjectToUniqueNameUsingObjectId @ConstraintObjectId, @NewName = @NewNameOfOriginalConstraint OUT;

  EXEC (@Cmd);

  EXEC tSQLt.Private_MarktSQLtTempObject @ObjectName = @QuotedFullConstraintName, @ObjectType = 'CONSTRAINT', @NewNameOfOriginalObject = @NewNameOfOriginalConstraint;
END; 
