CREATE FUNCTION tSQLt.[@tSQLt:SkipTest](@SkipReason NVARCHAR(MAX))
RETURNS TABLE
AS
RETURN
  SELECT 'EXEC tSQLt.Private_SkipTestAnnotationHelper @SkipReason = '''+
         ISNULL(NULLIF(REPLACE(@SkipReason,'''',''''''),''),'<no reason provided>')+
         ''';' AS AnnotationCmd;
