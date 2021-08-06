CREATE PROCEDURE [api].[Documents__UpdateAssignment]
	@AssignmentId INT,
	@Comment NVARCHAR(1024),
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT,
	@Culture NVARCHAR(50) = N'en',
	@NeutralCulture NVARCHAR(50) = N'en',
	@DocumentId INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;	

	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[Documents_Validate__UpdateAssignment] 
		@AssignmentId = @AssignmentId,
		@Comment = @Comment,
		@Top = @Top,
		@UserId = @UserId,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;
		
	EXEC [dal].[Documents__UpdateAssignment]
		@AssignmentId = @AssignmentId,
		@Comment = @Comment,
		@UserId = @UserId,
		@DocumentId = @DocumentId OUTPUT;
END;
