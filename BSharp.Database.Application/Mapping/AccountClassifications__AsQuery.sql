CREATE FUNCTION [map].[AccountClassifications__AsQuery] (
	@Entities [dbo].[AccountClassificationList] READONLY
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
		CAST(0 AS BIT) AS [IsDeprecated],
		CAST(1 AS BIT) AS [IsActive],
		NULL AS [ParentId],
		NULL As [Level],
		CAST(0 AS INT) As [ChildCount], 
		CAST(0 AS INT) As [ActiveChildCount], 
		NULL AS [Node],
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities E
);
