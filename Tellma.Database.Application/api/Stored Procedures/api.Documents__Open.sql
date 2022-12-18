CREATE PROCEDURE [api].[Documents__Open]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
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

	-- Once we fix the call to Lines_Validate__Transition_ToState in DOcuments_Validate_Open, we can remove this one
	EXEC [bll].[Lines_Validate__Transition_ToDocumentState]
		@Ids = @Ids, --documents
		@ToDocumentState = 0, -- 0: Open, 1:Close
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	EXEC [bll].[Documents_Validate__Open]
		@DefinitionId = @DefinitionId,
		@Ids = @Ids,
		@UserId = @UserId,
		@Top = @Top,
		@IsError = @IsError OUTPUT;		

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Execute
	EXEC [dal].[Documents__Open]
		@DefinitionId = @DefinitionId,
		@Ids = @Ids, 
		@UserId = @UserId;
END;
GO