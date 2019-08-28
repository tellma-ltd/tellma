CREATE PROCEDURE [dal].[Action_Views__Permissions]
	@Action NVARCHAR (255),
	@ViewIds [dbo].[StringList] READONLY
AS
DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
SELECT [ViewId], [Criteria], [Mask], [Action] FROM (

	-- Permissions in private roles that are assigned to the current user
	SELECT [ViewId], [Criteria], [Mask], [Action]
    FROM [dbo].[Permissions] AS P
    JOIN [dbo].[Roles] R ON P.[RoleId] = R.[Id]
    JOIN [dbo].[RoleMemberships] AS RM ON R.[Id] = RM.[RoleId]
    WHERE R.IsActive = 1 
    AND RM.[AgentId] = @UserId
    AND P.ViewId IN (SELECT [Code] FROM @ViewIds)

	UNION
	-- Permissions in public roles
    SELECT [ViewId], [Criteria], [Mask], [Action]
    FROM [dbo].[Permissions] P
    JOIN [dbo].[Roles] R ON P.RoleId = R.Id
    WHERE R.IsPublic = 1 
    AND R.IsActive = 1
    AND P.[ViewId] IN (SELECT [Code] FROM @ViewIds)

) AS E 
-- Any action implicitly includes the "Read" action,
-- The "All" action includes every other action
WHERE (@Action = N'Read' OR E.[Action] = @Action OR E.[Action] = 'All')