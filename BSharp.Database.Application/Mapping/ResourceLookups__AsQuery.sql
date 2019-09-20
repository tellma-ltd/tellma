CREATE FUNCTION [map].[ResourceLookups__AsQuery]
(	
	@DefinitionId NVARCHAR(255),
	@Entities [dbo].[ResourceLookupList] READONLY
)
RETURNS TABLE
AS
RETURN (
	SELECT 
		[Index] AS [Id],
		@DefinitionId AS [ResourceLookupDefinitionId],
		[Name],
		[Name2],
		[Name3],
		[Code],
		NULL AS [SortKey],
		1 AS [IsActive],
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities
);
