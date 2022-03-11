CREATE FUNCTION tSQLt.Private_GetDropItemCmd
(
/*SnipParamStart: CreateDropClassStatement.ps1*/
  @FullName NVARCHAR(MAX),
  @ItemType NVARCHAR(MAX)
/*SnipParamEnd: CreateDropClassStatement.ps1*/
)
RETURNS TABLE
AS
RETURN
/*SnipStart: CreateDropClassStatement.ps1*/
SELECT
    CASE @ItemType
      WHEN 'F' THEN 'ALTER TABLE '+(SELECT QUOTENAME(SCHEMA_NAME(schema_id))+'.'+QUOTENAME(OBJECT_NAME(parent_object_id)) FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(@FullName))+' '
      ELSE ''
    END+
    'DROP ' +
    CASE @ItemType 
      WHEN 'F' THEN 'CONSTRAINT'
      WHEN 'IF' THEN 'FUNCTION'
      WHEN 'TF' THEN 'FUNCTION'
      WHEN 'FN' THEN 'FUNCTION'
      WHEN 'FT' THEN 'FUNCTION'
      WHEN 'P' THEN 'PROCEDURE'
      WHEN 'PC' THEN 'PROCEDURE'
      WHEN 'SN' THEN 'SYNONYM'
      WHEN 'U' THEN 'TABLE'
      WHEN 'V' THEN 'VIEW'
      WHEN 'type' THEN 'TYPE'
      WHEN 'xml_schema_collection' THEN 'XML SCHEMA COLLECTION'
      WHEN 'schema' THEN 'SCHEMA'
     END+
     ' ' + 
     CASE @ItemType
       WHEN 'F' THEN QUOTENAME(OBJECT_NAME(OBJECT_ID(@FullName)))
       ELSE @FullName
     END+
     ';' AS cmd
/*SnipEnd: CreateDropClassStatement.ps1*/
