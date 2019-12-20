CREATE FUNCTION [map].[EntryClassifications__AsQuery] (
	@Entities [dbo].[EntryClassificationList] READONLY
)
RETURNS TABLE
AS
RETURN (
	SELECT 
		ISNULL(E.Id, 0) AS Id, 
		E.[Name], 
		E.[Name2], 
		E.[Name3], 
		E.[Code],
		E.[IsAssignable],
		E.[ForDebit],
		E.[ForCredit],
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
