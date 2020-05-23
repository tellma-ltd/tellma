CREATE PROCEDURE [api].[Roles__Save]
	@Roles [dbo].[RoleList] READONLY,
	@Members [dbo].[RoleMembershipList] READONLY,
	@Permissions [dbo].[PermissionList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN

	EXEC [bll].[Roles_Validate__Save]
		@Entities = @Roles,
		@Members = @Members,
		@Permissions = @Permissions,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;


	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	-- Validate business rules (read from the table)

	EXEC [dal].[Roles__Save]
		@Entities = @Roles,
		@Members = @Members,
		@Permissions = @Permissions,
		@ReturnIds = @ReturnIds
END;