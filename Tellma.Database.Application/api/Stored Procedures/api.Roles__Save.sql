CREATE PROCEDURE [api].[Roles__Save]
	@Entities [dbo].[RoleList] READONLY,
	@Members [dbo].[RoleMembershipList] READONLY,
	@Permissions [dbo].[PermissionList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[Roles_Validate__Save] 
		@Entities = @Entities,
		@Members = @Members,
		@Permissions = @Permissions,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[Roles__Save]
		@Entities = @Entities,
		@Members = @Members,
		@Permissions = @Permissions,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;
