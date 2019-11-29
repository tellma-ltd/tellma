CREATE PROCEDURE [dal].[Action_ViewPrefix__Permissions]
	@Action NVARCHAR (255),
	@ViewIdPrefix NVARCHAR (255)
AS
-- When changing this, remember to also change [dal].[Action_View__Permissions] and [dal].[GetUserPermissions] accordingly
DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
SELECT [ViewId], [Action], [Criteria], [Mask] FROM (

	-- Permissions in private roles that are assigned to the current user
	SELECT [ViewId], [Criteria], [Mask], [Action]
    FROM [dbo].[Permissions] AS P
    JOIN [dbo].[Roles] R ON P.[RoleId] = R.[Id]
    JOIN [dbo].[RoleMemberships] AS RM ON R.[Id] = RM.[RoleId]
    WHERE R.[IsActive] = 1 
    AND RM.[UserId] = @UserId
    AND (P.ViewId = N'all' OR P.[ViewId] LIKE @ViewIdPrefix + '%')

	UNION
	-- Permissions in public roles
    SELECT [ViewId], [Criteria], [Mask], [Action]
    FROM [dbo].[Permissions] P
    JOIN [dbo].[Roles] R ON P.[RoleId] = R.[Id]
    WHERE R.[IsPublic] = 1 
    AND R.[IsActive] = 1
    AND (P.ViewId = N'all' OR P.[ViewId] LIKE @ViewIdPrefix + '%')

) AS E 
-- Any action implicitly includes the "Read" action,
-- The "All" action includes every other action
WHERE (@Action = N'Read' OR E.[Action] = @Action OR E.[Action] = N'All')