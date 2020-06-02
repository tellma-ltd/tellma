CREATE PROCEDURE [api].[Roles__Save]
	@Entities [dbo].[RoleList] READONLY,
	@Members [dbo].[RoleMembershipList] READONLY,
	@Permissions [dbo].[PermissionList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN

	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[Roles_Validate__Save]
		@Entities = @Entities,
		@Members = @Members,
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
		@Entities = @Entities,
		@Members = @Members,
		@Permissions = @Permissions,
		@ReturnIds = @ReturnIds
END;