CREATE FUNCTION [bll].[ProductCategories__AsQuery] (
	@Entities [dbo].[ProductCategoryList] READONLY
)
RETURNS TABLE
AS
RETURN (
	SELECT 
		ISNULL(E.Id, 0) AS Id, 
		E.Name, 
		E.Name2, 
		E.Name3, 
		E.Code, 
		CAST(1 AS BIT) AS IsActive,
		E.ParentId,
		NULL As [Level],
		CAST(0 AS INT) As [ChildCount], 
		CAST(0 AS INT) As [ActiveChildCount], 
		(SELECT CAST([Node].ToString() + CAST(1 As NVARCHAR(MAX)) + N'/' As HIERARCHYID) FROM [dbo].[ProductCategories] WHERE Id = E.ParentId) As [Node],
		(SELECT [Node] FROM [dbo].[ProductCategories] WHERE Id = E.ParentId) As [ParentNode],
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities E
);
