CREATE FUNCTION [map].[ResponsibilityCenters__AsQuery] (
	@Entities [dbo].[ResponsibilityCenterList] READONLY
)
RETURNS TABLE
AS
RETURN (
	SELECT 
		ISNULL(E.Id, 0) AS Id, 
		E.[ResponsibilityType],
		E.IsLeaf,
	--	E.IsOperatingSegment,
		E.Name, 
		E.Name2, 
		E.Name3, 
		E.ManagerId,
		CAST(1 AS BIT) AS IsActive,
		E.ParentId,
		E.Code,
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById],
		(SELECT CAST([Node].ToString() + CAST(1 As NVARCHAR(MAX)) + N'/' As HIERARCHYID) FROM [dbo].[ResponsibilityCenters] WHERE Id = E.ParentId) As [Node],
		(SELECT [Node] FROM [dbo].[ResponsibilityCenters] WHERE Id = E.ParentId) As [ParentNode],
		NULL As [Level],
		CAST(0 AS INT) As [ChildCount], 
		CAST(0 AS INT) As [ActiveChildCount]
	FROM @Entities E
);
