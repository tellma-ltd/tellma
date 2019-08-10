CREATE PROCEDURE [api].[Documents__Save]
	@DocumentTypeId NVARCHAR(30),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[DocumentLineList] READONLY, 
	@Entries [dbo].[DocumentLineEntryList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	
	--Validate Domain rules
	--EXEC [bll].[Documents_Validate__Save]
	--	@Documents = @Documents,
	--	@Lines = @Lines,
	--	@Entries = @Entries,
	--	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	--SELECT * FROM @TransactionsLocal;
	--SELECT * FROM @WideLinesLocal;
	--SELECT * FROM @EntriesLocal;
	-- Validate business rules (read from the table)

	EXEC [dal].[Documents__Save]
		@DocumentTypeId = @DocumentTypeId,
		@Documents = @Documents,
		@Lines = @Lines,
		@Entries = @Entries,
		@ReturnIds = @ReturnIds;
END;