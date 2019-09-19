CREATE TYPE [dbo].[ResourceList] AS TABLE (
	[Index]							INT					PRIMARY KEY,
	[Id]							INT					NOT NULL DEFAULT 0,
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (255),
	[ResourceClassificationId]		INT,
	--[UnitMonetaryValue]				DECIMAL,
	[CurrencyId]					NCHAR (3),
	--[UnitMass]						DECIMAL,
	[MassUnitId]					INT,
	--[UnitVolume]						DECIMAL,
	[VolumeUnitId]					INT,
	--[UnitArea]						DECIMAL,
	[AreaUnitId]					INT,
	--[UnitLength]					DECIMAL,
	[LengthUnitId]					INT,
	--[UnitCount]						DECIMAL,
	[TimeUnitId]					INT,
	[CountUnitId]					INT,
	--[UnitTime]						DECIMAL,
 -- functional currency, common stock, basic, allowance, overtime/types, 
	--[SystemCode]					NVARCHAR (255),
	[Memo]							NVARCHAR (2048), -- description
	[CustomsReference]				NVARCHAR (255), -- how it is referred to by Customs
	--[PreferredSupplierId]			INT,			-- FK, Table Agents, specially for purchasing
	-- The following properties are user-defined, used for reporting
	[ResourceLookup1Id]				INT,			-- UDL 
	[ResourceLookup2Id]				INT,			-- UDL 
	[ResourceLookup3Id]				INT,			-- UDL 
	[ResourceLookup4Id]				INT,			-- UDL 
	INDEX IX_ResourceList__Code ([Code])
);