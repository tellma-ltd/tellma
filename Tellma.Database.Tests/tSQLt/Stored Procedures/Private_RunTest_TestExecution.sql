
CREATE PROCEDURE tSQLt.Private_RunTest_TestExecution
  @TestName NVARCHAR(MAX),
  @SetUp NVARCHAR(MAX),
  @CleanUp NVARCHAR(MAX),
  @NoTransactionFlag BIT,
  @TranName CHAR(32),
  @Result NVARCHAR(MAX) OUTPUT,
  @Msg NVARCHAR(MAX) OUTPUT,
  @TestEndTime DATETIME2 OUTPUT
AS
BEGIN
  DECLARE @TransactionStartedFlag BIT = 0;
  DECLARE @PreExecTrancount INT = NULL;
  DECLARE @TestExecutionCmd NVARCHAR(MAX) = 'EXEC ' + @TestName;
  DECLARE @CleanUpProcedureExecutionCmd NVARCHAR(MAX) = NULL;

    BEGIN TRY

      IF(@NoTransactionFlag = 0)
      BEGIN
        BEGIN TRAN;
        SET @TransactionStartedFlag = 1;
        SAVE TRAN @TranName;
      END;
      ELSE
      BEGIN
        SELECT object_id ObjectId, SCHEMA_NAME(schema_id) SchemaName, name ObjectName, type_desc ObjectType INTO #BeforeExecutionObjectSnapshot FROM sys.objects;
        EXEC tSQLt.Private_NoTransactionHandleTables @Action = 'Save';
      END;

      SET @PreExecTrancount = @@TRANCOUNT;
    
      DECLARE @TmpMsg NVARCHAR(MAX);
      SET @TestEndTime = NULL;
      BEGIN TRY
        IF (@SetUp IS NOT NULL)
        BEGIN
          EXEC @SetUp;
        END;

        EXEC (@TestExecutionCmd);

        IF(EXISTS(SELECT 1 FROM #ExpectException WHERE ExpectException = 1))
        BEGIN
          SET @TmpMsg = COALESCE((SELECT FailMessage FROM #ExpectException)+' ','')+'Expected an error to be raised.';
          EXEC tSQLt.Fail @TmpMsg;
        END
        SET @TestEndTime = SYSDATETIME();
      END TRY
      BEGIN CATCH
          SET @TestEndTime = ISNULL(@TestEndTime,SYSDATETIME());
          IF ERROR_MESSAGE() LIKE '%tSQLt.Failure%'
          BEGIN
              SELECT @Msg = Msg FROM #TestMessage;
              SET @Result = 'Failure';
          END
          ELSE
          BEGIN
            DECLARE @ErrorInfo NVARCHAR(MAX);
            SELECT @ErrorInfo = FormattedError FROM tSQLt.Private_GetFormattedErrorInfo();

            IF(EXISTS(SELECT 1 FROM #ExpectException))
            BEGIN
              DECLARE @ExpectException INT;
              DECLARE @ExpectedMessage NVARCHAR(MAX);
              DECLARE @ExpectedMessagePattern NVARCHAR(MAX);
              DECLARE @ExpectedSeverity INT;
              DECLARE @ExpectedState INT;
              DECLARE @ExpectedErrorNumber INT;
              DECLARE @FailMessage NVARCHAR(MAX);
              SELECT @ExpectException = ExpectException,
                     @ExpectedMessage = ExpectedMessage, 
                     @ExpectedSeverity = ExpectedSeverity,
                     @ExpectedState = ExpectedState,
                     @ExpectedMessagePattern = ExpectedMessagePattern,
                     @ExpectedErrorNumber = ExpectedErrorNumber,
                     @FailMessage = FailMessage
                FROM #ExpectException;

              IF(@ExpectException = 1)
              BEGIN
                SET @Result = 'Success';
                SET @TmpMsg = COALESCE(@FailMessage+' ','')+'Exception did not match expectation!';
                IF(ERROR_MESSAGE() <> @ExpectedMessage)
                BEGIN
                  SET @TmpMsg = @TmpMsg +CHAR(13)+CHAR(10)+
                             'Expected Message: <'+@ExpectedMessage+'>'+CHAR(13)+CHAR(10)+
                             'Actual Message  : <'+ERROR_MESSAGE()+'>';
                  SET @Result = 'Failure';
                END
                IF(ERROR_MESSAGE() NOT LIKE @ExpectedMessagePattern)
                BEGIN
                  SET @TmpMsg = @TmpMsg +CHAR(13)+CHAR(10)+
                             'Expected Message to be like <'+@ExpectedMessagePattern+'>'+CHAR(13)+CHAR(10)+
                             'Actual Message            : <'+ERROR_MESSAGE()+'>';
                  SET @Result = 'Failure';
                END
                IF(ERROR_NUMBER() <> @ExpectedErrorNumber)
                BEGIN
                  SET @TmpMsg = @TmpMsg +CHAR(13)+CHAR(10)+
                             'Expected Error Number: '+CAST(@ExpectedErrorNumber AS NVARCHAR(MAX))+CHAR(13)+CHAR(10)+
                             'Actual Error Number  : '+CAST(ERROR_NUMBER() AS NVARCHAR(MAX));
                  SET @Result = 'Failure';
                END
                IF(ERROR_SEVERITY() <> @ExpectedSeverity)
                BEGIN
                  SET @TmpMsg = @TmpMsg +CHAR(13)+CHAR(10)+
                             'Expected Severity: '+CAST(@ExpectedSeverity AS NVARCHAR(MAX))+CHAR(13)+CHAR(10)+
                             'Actual Severity  : '+CAST(ERROR_SEVERITY() AS NVARCHAR(MAX));
                  SET @Result = 'Failure';
                END
                IF(ERROR_STATE() <> @ExpectedState)
                BEGIN
                  SET @TmpMsg = @TmpMsg +CHAR(13)+CHAR(10)+
                             'Expected State: '+CAST(@ExpectedState AS NVARCHAR(MAX))+CHAR(13)+CHAR(10)+
                             'Actual State  : '+CAST(ERROR_STATE() AS NVARCHAR(MAX));
                  SET @Result = 'Failure';
                END
                IF(@Result = 'Failure')
                BEGIN
                  SET @Msg = @TmpMsg;
                END
              END 
              ELSE
              BEGIN
                  SET @Result = 'Failure';
                  SET @Msg = 
                    COALESCE(@FailMessage+' ','')+
                    'Expected no error to be raised. Instead this error was encountered:'+
                    CHAR(13)+CHAR(10)+
                    @ErrorInfo;
              END
            END;
            ELSE
            BEGIN
              SET @Result = 'Error';
              SET @Msg = @ErrorInfo;
            END; 
          END;
      END CATCH;
    END TRY
    BEGIN CATCH
        SET @Result = 'Error';
        SET @Msg = ERROR_MESSAGE();
    END CATCH

    --TODO:NoTran
    ---- Compare @@Trancount, throw up arms if it doesn't match
    --TODO:NoTran
    BEGIN TRY
      IF(@TransactionStartedFlag = 1)
      BEGIN
        ROLLBACK TRAN @TranName;
      END;
    END TRY
    BEGIN CATCH
        DECLARE @PostExecTrancount INT;
        SET @PostExecTrancount = @PreExecTrancount - @@TRANCOUNT;
        IF (@@TRANCOUNT > 0) ROLLBACK;
        BEGIN TRAN;
        IF(   @Result <> 'Success'
           OR @PostExecTrancount <> 0
          )
        BEGIN
          SELECT @Msg = COALESCE(@Msg, '<NULL>') + ' (There was also a ROLLBACK ERROR --> ' + FormattedError + ')' FROM tSQLt.Private_GetFormattedErrorInfo();
          SET @Result = 'Error';
        END;
    END CATCH;  
    IF (@NoTransactionFlag = 1)
    BEGIN
      SET @CleanUpProcedureExecutionCmd = (
        (
          SELECT 'EXEC tSQLt.Private_CleanUpCmdHandler ''EXEC '+ REPLACE(NT.CleanUpProcedureName,'''','''''') +';'', @Result OUT, @Msg OUT;'
            FROM #NoTransaction NT
           ORDER BY OrderId
             FOR XML PATH(''),TYPE
        ).value('.','NVARCHAR(MAX)')
      );
      IF(@CleanUpProcedureExecutionCmd IS NOT NULL)
      BEGIN
        EXEC sys.sp_executesql @CleanUpProcedureExecutionCmd, N'@Result NVARCHAR(MAX) OUTPUT, @Msg NVARCHAR(MAX) OUTPUT', @Result OUT, @Msg OUT;
      END;

      IF(@CleanUp IS NOT NULL)
      BEGIN
        EXEC tSQLt.Private_CleanUpCmdHandler @CleanUp, @Result OUT, @Msg OUT;
      END;

      DECLARE @CleanUpErrorMsg NVARCHAR(MAX);
      EXEC tSQLt.Private_CleanUp @FullTestName = @TestName, @Result = @Result OUT, @ErrorMsg = @CleanUpErrorMsg OUT;
      SET @Msg = @Msg + ISNULL(' ' + @CleanUpErrorMsg, '');

      SELECT object_id ObjectId, SCHEMA_NAME(schema_id) SchemaName, name ObjectName, type_desc ObjectType INTO #AfterExecutionObjectSnapshot FROM sys.objects;
      EXEC tSQLt.Private_AssertNoSideEffects
             @BeforeExecutionObjectSnapshotTableName ='#BeforeExecutionObjectSnapshot',
             @AfterExecutionObjectSnapshotTableName = '#AfterExecutionObjectSnapshot',
             @TestResult = @Result OUT,
             @TestMsg = @Msg OUT
    END;
    IF(@TransactionStartedFlag = 1)
    BEGIN
      COMMIT;
    END;
END;
