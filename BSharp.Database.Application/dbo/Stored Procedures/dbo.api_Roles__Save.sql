CREATE PROCEDURE [dbo].[api_Roles__Save]
	@Roles [dbo].[RoleList] READONLY,
	@Permissions [dbo].[PermissionList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [dbo].[Roles_Validate__Save]
		@Roles = @Roles,
		@Permissions = @Permissions;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	-- Validate business rules (read from the table)

	EXEC [dal].[Roles__Save]
		@Roles = @Roles,
		@Permissions = @Permissions
END;