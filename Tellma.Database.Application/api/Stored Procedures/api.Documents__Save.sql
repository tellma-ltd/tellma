CREATE PROCEDURE [api].[Documents__Save]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].[EntryList] READONLY,
	@Attachments [dbo].[AttachmentList] READONLY,
	@ReturnIds BIT = 0,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT,
	@Culture NVARCHAR(50) = N'en',
	@NeutralCulture NVARCHAR(50) = N'en'
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;

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
		@Top = @Top,
		@IsError = @IsError OUTPUT;	

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
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
GO