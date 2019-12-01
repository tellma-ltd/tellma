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
		-- E.ParentId, (SELECT [Code] FROM dbo.[EntryClassifications] WHERE [Node] = E.[ParentNode]) AS ParentId,
		NULL As [Level],
		CAST(0 AS INT) As [ChildCount], 
		CAST(0 AS INT) As [ActiveChildCount], 
		-- TODO: Ask Ahmad what does he want from these subsequent two lines
	--	(SELECT CAST([Node].ToString() + CAST(1 As NVARCHAR(MAX)) + N'/' As HIERARCHYID) FROM [dbo].[EntryClassifications] WHERE Id = E.ParentId) As [Node],
	--	(SELECT [Node] FROM [dbo].[EntryClassifications] WHERE Id = E.ParentId) As [ParentNode],
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities E
);
