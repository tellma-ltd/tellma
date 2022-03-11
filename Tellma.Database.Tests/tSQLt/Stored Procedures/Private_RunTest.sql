
CREATE PROCEDURE tSQLt.Private_RunTest
   @TestName NVARCHAR(MAX),
   @SetUp NVARCHAR(MAX) = NULL,
   @CleanUp NVARCHAR(MAX) = NULL
AS
BEGIN
    DECLARE @OuterPerimeterTrancount INT = @@TRANCOUNT;

    DECLARE @Msg NVARCHAR(MAX); SET @Msg = '';
    DECLARE @Msg2 NVARCHAR(MAX); SET @Msg2 = '';
    DECLARE @TestClassName NVARCHAR(MAX); SET @TestClassName = '';
    DECLARE @TestProcName NVARCHAR(MAX); SET @TestProcName = '';
    DECLARE @Result NVARCHAR(MAX);
    DECLARE @TranName CHAR(32) = NULL;
    DECLARE @TestResultId INT;
    DECLARE @TestObjectId INT;
    DECLARE @TestEndTime DATETIME2 = NULL;

    DECLARE @VerboseMsg NVARCHAR(MAX);
    DECLARE @Verbose BIT;
    SET @Verbose = ISNULL((SELECT CAST(Value AS BIT) FROM tSQLt.Private_GetConfiguration('Verbose')),0);
    
    TRUNCATE TABLE tSQLt.CaptureOutputLog;
    CREATE TABLE #TestMessage(Msg NVARCHAR(MAX));
    CREATE TABLE #ExpectException(ExpectException INT,ExpectedMessage NVARCHAR(MAX), ExpectedSeverity INT, ExpectedState INT, ExpectedMessagePattern NVARCHAR(MAX), ExpectedErrorNumber INT, FailMessage NVARCHAR(MAX));
    CREATE TABLE #SkipTest(SkipTestMessage NVARCHAR(MAX) DEFAULT '');
    CREATE TABLE #NoTransaction(OrderId INT IDENTITY(1,1),CleanUpProcedureName NVARCHAR(MAX));
    CREATE TABLE #TableBackupLog(OriginalName NVARCHAR(MAX), BackupName NVARCHAR(MAX));


    IF EXISTS (SELECT 1 FROM sys.extended_properties WHERE name = N'SetFakeViewOnTrigger')
    BEGIN
      RAISERROR('Test system is in an invalid state. SetFakeViewOff must be called if SetFakeViewOn was called. Call SetFakeViewOff after creating all test case procedures.', 16, 10) WITH NOWAIT;
      RETURN -1;
    END;

    
    SELECT @TestClassName = OBJECT_SCHEMA_NAME(OBJECT_ID(@TestName)),
           @TestProcName = tSQLt.Private_GetCleanObjectName(@TestName),
           @TestObjectId = OBJECT_ID(@TestName);
           
    INSERT INTO tSQLt.TestResult(Class, TestCase, TranName, Result) 
        SELECT @TestClassName, @TestProcName, @TranName, 'A severe error happened during test execution. Test did not finish.'
        OPTION(MAXDOP 1);
    SELECT @TestResultId = SCOPE_IDENTITY();

    IF(@Verbose = 1)
    BEGIN
      SET @VerboseMsg = 'tSQLt.Run '''+@TestName+'''; --Starting';
      EXEC tSQLt.Private_Print @Message =@VerboseMsg, @Severity = 0;
    END;


    SET @Result = 'Success';
    DECLARE @SkipTestFlag BIT = 0;
    DECLARE @NoTransactionFlag BIT = 0;

    BEGIN TRY
      EXEC tSQLt.Private_ProcessTestAnnotations @TestObjectId=@TestObjectId;
      SET @SkipTestFlag = CASE WHEN EXISTS(SELECT 1 FROM #SkipTest) THEN 1 ELSE 0 END;
      SET @NoTransactionFlag = CASE WHEN EXISTS(SELECT 1 FROM #NoTransaction) THEN 1 ELSE 0 END;

      IF(@SkipTestFlag = 0)
      BEGIN
        IF(@NoTransactionFlag = 0)
        BEGIN
          EXEC tSQLt.GetNewTranName @TranName OUT;
          UPDATE tSQLt.TestResult SET TranName = @TranName WHERE Id = @TestResultId;
        END;
        EXEC tSQLt.Private_RunTest_TestExecution
          @TestName,
          @SetUp,
          @CleanUp,
          @NoTransactionFlag,
          @TranName,
          @Result OUT,
          @Msg OUT,
          @TestEndTime OUT;

      END;
      ELSE
      BEGIN
        DECLARE @TmpMsg NVARCHAR(MAX);
        SELECT 
            @Result = 'Skipped',
            @Msg = ST.SkipTestMessage 
          FROM #SkipTest AS ST;
        SET @TmpMsg = '-->'+@TestName+' skipped: '+@Msg;
        EXEC tSQLt.Private_Print @Message = @TmpMsg;
        SET @TestEndTime = SYSDATETIME();
      END;
    END TRY
    BEGIN CATCH
      SET @Result = 'Error';
      SET @Msg = ISNULL(NULLIF(@Msg,'') + ' ','')+ERROR_MESSAGE();
      --SET @TestEndTime = SYSDATETIME();
    END CATCH;
----------------------------------------------------------------------------------------------
    If(@Result NOT IN ('Success','Skipped'))
    BEGIN
      SET @Msg2 = @TestName + ' failed: (' + @Result + ') ' + @Msg;
      EXEC tSQLt.Private_Print @Message = @Msg2, @Severity = 0;
    END;
    IF EXISTS(SELECT 1 FROM tSQLt.TestResult WHERE Id = @TestResultId)
    BEGIN
        UPDATE tSQLt.TestResult SET
            Result = @Result,
            Msg = @Msg,
            TestEndTime = @TestEndTime
         WHERE Id = @TestResultId;
    END;
    ELSE
    BEGIN
        INSERT tSQLt.TestResult(Class, TestCase, TranName, Result, Msg)
        SELECT @TestClassName, 
               @TestProcName,  
               '?', 
               'Error', 
               'TestResult entry is missing; Original outcome: ' + @Result + ', ' + @Msg;
    END;    

    IF(@Verbose = 1)
    BEGIN
      SET @VerboseMsg = 'tSQLt.Run '''+@TestName+'''; --Finished';
      EXEC tSQLt.Private_Print @Message =@VerboseMsg, @Severity = 0;
      --DECLARE @AsciiArtLine NVARCHAR(MAX) = CASE WHEN @Result<>'Success' THEN REPLICATE(CHAR(168),150)+' '+CHAR(155)+CHAR(155)+' '+@Result + ' ' +CHAR(139)+CHAR(139) ELSE '' END + CHAR(13)+CHAR(10) + CHAR(173);
      --EXEC tSQLt.Private_Print @Message = @AsciiArtLine, @Severity = 0;
    END;

    IF(@Result = 'FATAL')
    BEGIN
      INSERT INTO tSQLt.Private_Seize VALUES(1);   
      RAISERROR('The last test has invalidated the current installation of tSQLt. Please reinstall tSQLt.',16,10);
    END;
    IF(@Result = 'Abort')
    BEGIN
      RAISERROR('Aborting the current execution of tSQLt due to a severe error.', 16, 10);
    END;

    IF(@OuterPerimeterTrancount != @@TRANCOUNT) RAISERROR('tSQLt is in an invalid state: Stopping Execution. (Mismatching TRANCOUNT: %i <> %i))',16,10,@OuterPerimeterTrancount, @@TRANCOUNT);

END;
