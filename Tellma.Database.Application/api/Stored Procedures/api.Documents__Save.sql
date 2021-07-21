CREATE PROCEDURE [api].[Documents__Save]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].[EntryList] READONLY,
	@Attachments [dbo].[AttachmentList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT,
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	-- Set the global values of the session context
	DECLARE @UserLanguageIndex TINYINT = [dbo].[fn_User__Language](@Culture, @NeutralCulture);
    EXEC sys.sp_set_session_context @key = N'UserLanguageIndex', @value = @UserLanguageIndex;

	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[Documents_Validate__Save] 
		@DefinitionId = @DefinitionId,
		@Documents = @Documents,
		@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines,
		@Entries = @Entries,
		@Attachments = @Attachments,
		@UserId = @UserId,
		@IsError = @IsError OUTPUT;
		

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Execute
	EXEC [dal].[Documents__SaveAndRefresh]
		@DefinitionId = @DefinitionId,
		@Documents = @Documents,
		@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines,
		@Entries = @Entries,
		@Attachments = @Attachments,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;