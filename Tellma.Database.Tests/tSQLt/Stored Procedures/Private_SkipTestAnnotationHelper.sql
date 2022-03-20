CREATE PROCEDURE tSQLt.Private_SkipTestAnnotationHelper
  @SkipReason NVARCHAR(MAX)
AS
BEGIN
  INSERT INTO #SkipTest VALUES(@SkipReason);
END;
