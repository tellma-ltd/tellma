CREATE PROCEDURE [api].[Views__Save]
	@Views [dbo].[ViewList] READONLY,
	@Permissions [dbo].[PermissionList] READONLY, 
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT,
	@ReturnIds BIT = 0
AS
BEGIN
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [bll].[Views_Validate__Save]
		@Views = @Views,
		@Permissions = @Permissions;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Views__Save]
		@Views = @Views,
		@Permissions = @Permissions;
END;