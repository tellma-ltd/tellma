CREATE FUNCTION [map].[Resources__AsQuery]
(	
	@DefinitionId NVARCHAR(255),
	@Entities [dbo].[ResourceList] READONLY
)
RETURNS TABLE
AS
RETURN (
	SELECT 
		[Index] AS [Id],
	--	[OperatingSegmentId],
		@DefinitionId AS [ResourceDefinitionId],
		[Name],
		[Name2],
		[Name3],
		[Identifier],
		[Code],
		[ResourceClassificationId],
		[CountUnitId],
		[CurrencyId],
		[MonetaryValue],
		[MassUnitId],
		[Mass],
		[VolumeUnitId],
		[Volume]
		[TimeUnitId],
		[Time],
		[Description],
		--[AttachmentsFolderURL]			NVARCHAR (255), 
		--[CustomsReference]				NVARCHAR (255), -- how it is referred to by Customs
		--[PreferredSupplierId]			INT,-- FK, Table Agents, specially for purchasing
		-- The following properties are user-defined, used for reporting
		[AvailableSince],
		[AvailableTill],
		--[UniqueReference1]				NVARCHAR(50), -- such as VIN, UPC, EPC, etc...
		--[UniqueReference2]				NVARCHAR(50), -- such as Engine number
		--[UniqueReference3]				NVARCHAR(50), -- such as Plate number
		--[PreferredSupplierId],
		[Lookup1Id],
		[Lookup2Id],
		--[Lookup3Id],
		--[Lookup4Id],
		1 AS [IsActive],
		SYSDATETIMEOFFSET() AS [CreatedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [CreatedById],
		SYSDATETIMEOFFSET() AS [ModifiedAt],
		CONVERT(INT, SESSION_CONTEXT(N'UserId')) AS [ModifiedById]
	FROM @Entities
);
