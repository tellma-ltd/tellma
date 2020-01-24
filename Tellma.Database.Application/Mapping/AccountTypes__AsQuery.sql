CREATE FUNCTION [map].[AccountTypes__AsQuery] (
	@Entities [dbo].[AccountTypeList] READONLY
)
RETURNS TABLE
AS
RETURN (
	SELECT 
		ISNULL(E.Id, 0) AS Id, 
		E.[Name], 
		E.[Name2], 
		E.[Name3], 
		E.[Description],
		E.[Description2],
		E.[Description3],
		E.[Code],
		E.[IsAssignable],
		E.[IsCurrent],
		E.[IsReal],
		E.[IsResourceClassification],
		E.[IsPersonal],
		E.[EntryTypeParentId],
		CAST(1 AS BIT) AS IsActive,
		NULL As [Level],
		CAST(0 AS INT) As [ChildCount], 
		CAST(0 AS INT) As [ActiveChildCount],
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities E
);	