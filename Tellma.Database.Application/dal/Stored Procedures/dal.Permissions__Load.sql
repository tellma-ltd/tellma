﻿-- Returns all the permissions of a specific user
CREATE PROCEDURE [dal].[Permissions__Load]
	@UserId INT
AS
-- When changing this, remember to also change [dal].[Action_View__Permissions] and [dal].[Action_ViewPrefix__Permissions]
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
    
	-- Return Report Ids shared with this user
	SELECT DISTINCT D.[Id] FROM [dbo].[ReportDefinitions] D
	JOIN [dbo].[ReportDefinitionRoles] DR ON D.[Id] = DR.[ReportDefinitionId]
	JOIN [dbo].[Roles] R ON DR.[RoleId] = R.[Id]
	LEFT JOIN [dbo].[RoleMemberships] RM ON RM.[RoleId] = R.[Id]
	WHERE D.[ShowInMainMenu] = 1 AND R.[IsActive] = 1 AND (RM.[UserId] = @UserId OR R.[IsPublic] = 1)
    
	-- Return Dashboard Ids shared with this user
	SELECT DISTINCT D.[Id] FROM [dbo].[DashboardDefinitions] D
	JOIN [dbo].[DashboardDefinitionRoles] DR ON D.[Id] = DR.[DashboardDefinitionId]
	JOIN [dbo].[Roles] R ON DR.[RoleId] = R.[Id]
	LEFT JOIN [dbo].[RoleMemberships] RM ON RM.[RoleId] = R.[Id]
	WHERE D.[ShowInMainMenu] = 1 AND R.[IsActive] = 1 AND (RM.[UserId] = @UserId OR R.[IsPublic] = 1)
    
	-- Return Printing Ids shared with this user
	SELECT DISTINCT D.[Id] FROM [dbo].[PrintingTemplates] D
	JOIN [dbo].[PrintingTemplateRoles] DR ON D.[Id] = DR.[PrintingTemplateId]
	JOIN [dbo].[Roles] R ON DR.[RoleId] = R.[Id]
	LEFT JOIN [dbo].[RoleMemberships] RM ON RM.[RoleId] = R.[Id]
	WHERE D.[ShowInMainMenu] = 1 AND R.[IsActive] = 1 AND (RM.[UserId] = @UserId OR R.[IsPublic] = 1)
