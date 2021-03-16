-- Returns all the permissions of the current user
CREATE PROCEDURE [dal].[Permissions__Load]
	@IncludeReportAndDashboardIds BIT = 1
AS
-- When changing this, remember to also change [dal].[Action_View__Permissions] and [dal].[Action_ViewPrefix__Permissions]
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- Return the version
    SELECT [PermissionsVersion] 
    FROM [dbo].[Users]
    WHERE [Id] = @UserId

	-- Return the permissions
    SELECT [View], [Action], [Criteria], [Mask] 
    FROM [dbo].[Permissions] P
    JOIN [dbo].[Roles] R ON P.RoleId = R.Id
    JOIN [dbo].[RoleMemberships] RM ON R.Id = RM.[RoleId]
    WHERE R.[IsActive] = 1 
    AND RM.[UserId] = @UserId
    UNION
    SELECT [View], [Action], [Criteria], [Mask] 
    FROM [dbo].[Permissions] P
    JOIN [dbo].[Roles] R ON P.[RoleId] = R.Id
    WHERE R.[IsPublic] = 1 
    AND R.[IsActive] = 1
    
	-- Return Dashboard Ids shared with this user
    IF (@IncludeReportAndDashboardIds = 1)    
	    SELECT DISTINCT D.[Id] FROM [dbo].[DashboardDefinitions] D
	    JOIN [dbo].[DashboardDefinitionRoles] DR ON D.[Id] = DR.[DashboardDefinitionId]
	    JOIN [dbo].[Roles] R ON DR.[RoleId] = R.[Id]
	    LEFT JOIN [dbo].[RoleMemberships] RM ON RM.[RoleId] = R.[Id]
	    WHERE D.[ShowInMainMenu] = 1 AND R.[IsActive] = 1 AND (RM.[UserId] = @UserId OR R.[IsPublic] = 1)