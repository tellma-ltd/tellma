CREATE FUNCTION [bll].[Resources__AsQuery]
(	
	@DefinitionId NVARCHAR(255),
	@Entities [dbo].[ResourceList] READONLY
)
RETURNS TABLE
AS
RETURN (
	SELECT 
		[Index] AS [Id],
		@DefinitionId AS [ResourceDefinitionId],
		[Name],
		[Name2],
		[Name3],
		[Code],
		[ResourceClassificationId],
		--[UnitMonetaryValue],
		[CurrencyId],
		--[UnitMass],
		[MassUnitId],
		--[UnitVolume],
		[VolumeUnitId],
		--[UnitArea],
		[AreaUnitId],
		--[UnitLength],
		[LengthUnitId],
		--[UnitCount],
		[TimeUnitId],
		[CountUnitId],
		--[UnitTime],
		--[SystemCode],
		[Memo],
		[CustomsReference],
		--[PreferredSupplierId],
		[ResourceLookup1Id],
		[ResourceLookup2Id],
		[ResourceLookup3Id],
		[ResourceLookup4Id],
		1 AS [IsActive],
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities
);
