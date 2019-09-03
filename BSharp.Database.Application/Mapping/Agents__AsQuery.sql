CREATE FUNCTION [bll].[Agents__AsQuery]
(	
@Entities [dbo].[AgentList] READONLY
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
		[AgentType],
		[IsRelated],
		[PreferredLanguage],
		NULL AS [ImageId],
		1 AS [IsActive],
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities
);
