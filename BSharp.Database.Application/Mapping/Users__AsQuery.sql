CREATE FUNCTION [dbo].[Users__AsQuery]
(
	@Entities [dbo].[UserList] READONLY
)
RETURNS TABLE
AS
RETURN (
	SELECT 
		[Index] AS [Id],
		NULL AS [Name],
		NULL AS [Name2],
		NULL AS [Name3],
		NULL AS [ImageId],
		[Email],
		NULL AS [ExternalId],										-- TODO join and read from table
		N'New' AS [State],											-- TODO join and read from table
		NULL AS [LastAccess],										-- TODO join and read from table
		SYSDATETIMEOFFSET() AS [CreatedAt],							-- TODO join and read from table
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],	-- TODO join and read from table
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities
)
