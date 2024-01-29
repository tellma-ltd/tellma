CREATE PROCEDURE [api].[Documents__Close]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT,
	@Culture NVARCHAR(50) = N'en',
	@NeutralCulture NVARCHAR(50) = N'en',
    @PreviousInvoiceSerialNumber INT OUTPUT,
    @PreviousInvoiceHash NVARCHAR(MAX) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;
	
	-- (1) Validate
	DECLARE @IsError BIT;
	
	EXEC [bll].[Lines_Validate__Transition_ToDocumentState]
		@Ids = @Ids, --documents
		@ToDocumentState = 1, -- 0: Open, 1:Close
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;
		
	EXEC [bll].[Documents_Validate__Close]
		@DefinitionId = @DefinitionId,
		@Ids = @Ids,
		@UserId = @UserId,
		@Top = @Top,
		@IsError = @IsError OUTPUT;		

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Execute
	EXEC [dal].[Documents__Close]
		@DefinitionId = @DefinitionId,
		@Ids = @Ids, 
		@UserId = @UserId,
		@PreviousInvoiceSerialNumber = @PreviousInvoiceSerialNumber,
		@PreviousInvoiceHash = @PreviousInvoiceHash;
END;
GO