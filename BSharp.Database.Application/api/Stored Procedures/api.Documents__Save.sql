CREATE PROCEDURE [api].[Documents__Save]
	@DocumentTypeId NVARCHAR(30),
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[DocumentLineList] READONLY, 
	@Entries [dbo].[DocumentLineEntryList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @FilledEntries [dbo].[DocumentLineEntryList]
	
	INSERT INTO @FilledEntries
	EXEC bll.DocumentLineEntries__Fill
		@Documents = @Documents,
		@Lines = @Lines,
		@Entries = @Entries;

	--SELECT * FROM @FilledEntries;
	--INSERT INTO @ValidationErrors
	--EXEC [bll].[Documents_Validate__Save]
	--	@Documents = @Documents,
	--	@Lines = @Lines,
	--	@Entries = @FilledEntries;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Documents__Save]
		@DocumentTypeId = @DocumentTypeId,
		@Documents = @Documents,
		@Lines = @Lines,
		@Entries = @FilledEntries,
		@ReturnIds = @ReturnIds;
END;