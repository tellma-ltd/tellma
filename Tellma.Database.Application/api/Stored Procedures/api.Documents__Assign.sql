CREATE PROCEDURE [api].[Documents__Assign] 
	@Ids [dbo].[IndexedIdList] READONLY,
	@AssigneeId INT,
	@Comment NVARCHAR(1024),
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT,
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;
	
	-- (1) Validate
	DECLARE @IsError BIT;
	EXEC [bll].[Documents_Validate__Assign]
		@Ids = @Ids,
		@AssigneeId = @AssigneeId,
		@Comment = @Comment,
		@UserId = @UserId,
		@Top = @Top,
		@IsError = @IsError OUTPUT;		

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Execute
	EXEC [dal].[Documents__Assign]
		@Ids = @Ids, 
		@AssigneeId = @AssigneeId,
		@Comment = @Comment,
		@ManualAssignment = 1,
		@UserId = @UserId;
END;