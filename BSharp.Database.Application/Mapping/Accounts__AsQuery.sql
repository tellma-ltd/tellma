CREATE FUNCTION [map].[Accounts__AsQuery]
(	
	@DefinitionId NVARCHAR(50),
	@Entities [dbo].[AccountList] READONLY
)
RETURNS TABLE
AS
RETURN (
	SELECT 
		[Index] AS [Id],
		[AccountTypeId],
		[AccountClassificationId],
		[Name],
		[Name2],
		[Name3],
		[Code],
		[PartyReference],
		[ResponsibilityCenterId],
		[CustodianId],
		[ResourceId],
		[LocationId],
		@DefinitionId AS [AccountDefinitionId],
		CAST(0 AS BIT) AS [IsDeprecated],
		CAST(1 AS BIT) AS [IsActive],
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities
);
