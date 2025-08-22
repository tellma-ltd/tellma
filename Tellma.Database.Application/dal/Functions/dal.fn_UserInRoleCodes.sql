CREATE FUNCTION [dal].[fn_UserInRoleCodes]
(
	@UserId INT,
	@RoleCodes StringList READONLY
)
RETURNS BIT
AS
BEGIN
	IF EXISTS(
		SELECT *
		FROM dbo.RoleMemberships RM
		JOIN dbo.Roles R ON R.Id = RM.[RoleId]
		WHERE RM.[UserId] = @UserId 
		AND R.[Code] IN (SELECT Id FROM @RoleCodes)
	)
		RETURN 1;

	RETURN 0;
END