CREATE PROCEDURE [dbo].[api_Documents__Save]
	@Documents [dbo].[DocumentList] READONLY,
	@WideLines [dbo].[DocumentWideLineList] READONLY, 
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	DECLARE @Ids [dbo].[IdList];
	DECLARE @ResultJson NVARCHAR(MAX);
	DECLARE @Lines dbo.[DocumentLineList];
	DECLARE @Entries dbo.DocumentLineEntryList;
	
	EXEC [dbo].[bll_DocumentWideLines__Unpivot] -- UI logic to fill missing fields, and unpivot
		@WideLines = @WideLines,
		@ResultJson = @ResultJson OUTPUT;

/* TODO: Needs to debug the two lines
	INSERT INTO @Lines SELECT * FROM dbo.[fw_TransactionLines__Json](@ResultJson);
	INSERT INTO @Entries SELECT * FROM dbo.[fw_TransactionEntries__Json](@ResultJson);
*/
	--Validate Domain rules
	EXEC [dbo].[bll_Documents_Validate__Save]
		@Documents = @Documents,
		@Entries = @Entries,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	--SELECT * FROM @TransactionsLocal;
	--SELECT * FROM @WideLinesLocal;
	--SELECT * FROM @EntriesLocal;
	-- Validate business rules (read from the table)

	EXEC [dbo].[dal_Document__Save]
		@Documents = @Documents,
		@Lines = @Lines,
		@Entries = @Entries
END;