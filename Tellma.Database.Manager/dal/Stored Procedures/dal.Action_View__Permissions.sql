CREATE PROCEDURE [dal].[Action_View__Permissions]
    @UserId INT,
	@Action NVARCHAR (255),
	@View NVARCHAR (255)
AS
BEGIN
    -- When changing this, remember to also change [dal].[Permissions__Load]
    SELECT [View], [Action], [Criteria] FROM (

	    SELECT [View], [Criteria], [Action]
        FROM [dbo].[AdminPermissions] AS P
        WHERE P.[AdminUserId] = @UserId
        AND (P.[View] = N'all' OR P.[View] = @View)

    ) AS E 
    -- Any action implicitly includes the "Read" action,
    -- The "All" action includes every other action
    WHERE (@Action = N'Read' OR E.[Action] = @Action OR E.[Action] = N'All')
END