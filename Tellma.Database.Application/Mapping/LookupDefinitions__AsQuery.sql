CREATE FUNCTION [map].[LookupDefinitions__AsQuery] (
	@Entities [dbo].[LookupDefinitionList] READONLY
)
RETURNS TABLE
AS
RETURN (
	SELECT 
		[Index] AS [Id],
		[TitleSingular],
		[TitleSingular2],
		[TitleSingular3],		
		[TitlePlural],
		[TitlePlural2],
		[TitlePlural3],
		[MainMenuIcon],
		[MainMenuSection],
		[MainMenuSortKey],
		'Draft' AS [State],
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities
);
