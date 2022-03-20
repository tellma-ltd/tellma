CREATE PROCEDURE tSQLt.DefaultResultFormatter
AS
BEGIN
    DECLARE @TestList NVARCHAR(MAX);
    DECLARE @Dashes NVARCHAR(MAX);
    DECLARE @CountSummaryMsg NVARCHAR(MAX);
    DECLARE @NewLine NVARCHAR(MAX);
    DECLARE @IsSuccess INT;
    DECLARE @SuccessCnt INT;
    DECLARE @Severity INT;
    DECLARE @SummaryError INT;
    
    SELECT *
      INTO #TestResultOutput
      FROM tSQLt.Private_PrepareTestResultForOutput() AS PTRFO;
    
    EXEC tSQLt.TableToText @TestList OUTPUT, '#TestResultOutput', 'No';

    SELECT @CountSummaryMsg = Msg, 
           @IsSuccess = 1 - SIGN(FailCnt + ErrorCnt),
           @SuccessCnt = SuccessCnt
      FROM tSQLt.TestCaseSummary();
      
    SELECT @SummaryError = CAST(PC.Value AS INT)
      FROM tSQLt.Private_Configurations AS PC
     WHERE PC.Name = 'SummaryError';

    SELECT @Severity = 16*(1-@IsSuccess);
    IF(@SummaryError = 0)
    BEGIN
      SET @Severity = 0;
    END;
    
    SELECT @Dashes = REPLICATE('-',LEN(@CountSummaryMsg)),
           @NewLine = CHAR(13)+CHAR(10);
    
    
    EXEC tSQLt.Private_Print @NewLine,0;
    EXEC tSQLt.Private_Print '+----------------------+',0;
    EXEC tSQLt.Private_Print '|Test Execution Summary|',0;
    EXEC tSQLt.Private_Print '+----------------------+',0;
    EXEC tSQLt.Private_Print @NewLine,0;
    EXEC tSQLt.Private_Print @TestList,0;
    EXEC tSQLt.Private_Print @Dashes,0;
    EXEC tSQLt.Private_Print @CountSummaryMsg, @Severity;
    EXEC tSQLt.Private_Print @Dashes,0;
END;
