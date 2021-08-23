CREATE PROCEDURE [api].[Users__Save]
	@Entities [UserList] READONLY,
	@Roles [dbo].[RoleMembershipList] READONLY,
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
	EXEC [bll].[Users_Validate__Save] 
		@Entities = @Entities,
		@Roles = @Roles,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Save
	EXEC [dal].[Users__Save]
		@Entities = @Entities,
		@Roles = @Roles,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;