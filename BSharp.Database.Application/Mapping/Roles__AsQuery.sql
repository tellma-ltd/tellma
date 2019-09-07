CREATE FUNCTION [bll].[Roles__AsQuery]
(	
@Entities [dbo].[RoleList] READONLY
)
RETURNS TABLE
AS
RETURN (
	SELECT 
		[Index] AS [Id],
		[Name],
		[Name2],
		[Name3],
		[Code],
		[IsPublic],
		1 AS [IsActive],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [SavedById]
	FROM @Entities
);
