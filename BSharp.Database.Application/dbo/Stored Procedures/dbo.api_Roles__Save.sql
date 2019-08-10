CREATE PROCEDURE [dbo].[api_Roles__Save]
	@Roles [dbo].[RoleList] READONLY,
	@Permissions [dbo].[PermissionList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	--Validate Domain rules
	EXEC [dbo].[bll_Roles_Validate__Save]
		@Roles = @Roles,
		@Permissions = @Permissions,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	-- Validate business rules (read from the table)

	EXEC [dbo].[dal_Roles__Save]
		@Roles = @Roles,
		@Permissions = @Permissions
END;