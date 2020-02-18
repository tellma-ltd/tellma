CREATE PROCEDURE [dal].[Permissions__Load]
AS
-- When changing this, remember to also change [dal].[Action_View__Permissions]
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Return the version
    SELECT [PermissionsVersion] 
    FROM [dbo].[AdminUsers]
    WHERE [Id] = @UserId

	-- Return the permissions
    SELECT [View], [Action], [Criteria]
    FROM [dbo].[AdminPermissions] P
    WHERE P.[AdminUserId] = @UserId

