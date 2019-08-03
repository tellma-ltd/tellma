CREATE PROCEDURE [dbo].[api_Views__Save]
	@Views [dbo].[ViewList] READONLY,
	@Permissions [dbo].[PermissionList] READONLY, 
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	--Validate Domain rules
	EXEC [dbo].[bll_Views_Validate__Save]
		@Views = @Views,
		@Permissions = @Permissions,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dbo].[dal_Views__Save]
		@Views = @Views,
		@Permissions = @Permissions;
END;