CREATE FUNCTION [bll].[MeasurementUnits__AsQuery] (
	@Entities [dbo].[MeasurementUnitList] READONLY
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
		[UnitType],
		[UnitAmount],
		[BaseAmount],
		1 AS [IsActive],
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities
);
