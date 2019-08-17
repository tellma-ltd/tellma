CREATE TYPE [dbo].[ResourceList] AS TABLE (
	[Index]						INT					PRIMARY KEY IDENTITY(0, 1),
	[Id]						INT					NOT NULL DEFAULT 0,
	[ResourceType]				NVARCHAR (255)		NOT NULL,
	[Name]						NVARCHAR (255)		NOT NULL,
	[Name2]						NVARCHAR (255),
	[Name3]						NVARCHAR (255),
	[Uniqueness]				TINYINT				NOT NULL DEFAULT 0,
	[IsBatch]					BIT					NOT NULL DEFAULT 0,
	[ValueMeasure]				NVARCHAR (255) NOT NULL, -- Currency, Mass, Volumne, Length, Count, Time, 
	[UnitId]					INT,
	[CurrencyId]				INT,			-- the unit If the resource has a financial meaure assigned to it.
	[UnitMoney]					DECIMAL,		-- if not null, it specifies the Cost per Unit
	[MassUnitId]				INT,			-- the unit If the resource has a mass measure assigned to it.
	[UnitMass]					DECIMAL,		-- if not null, it specifies the conversion rate Mass/Count
	[VolumeUnitId]				INT,			-- FK, Table Units
	[UnitVolume]				DECIMAL,		-- if not null, it specifies the conversion rate Volume/Count
	[AreaUnitId]				INT,			-- FK, Table Units
	[UnitArea]					DECIMAL,		-- if not null, it specifies the conversion rate Area/Count
	[LengthUnitId]				INT,			-- FK, Table Units
	[UnitLength]				DECIMAL,		-- if not null, it specifies the conversion rate Length/Count
	[TimeUnitId]				INT,			-- FK, Table Units
	[UnitTime]					DECIMAL,		-- if not null, it specifies the conversion rate Time/Count
	[CountUnitId]				INT,			-- FK, Table Units
	[Code]						NVARCHAR (255),
 -- functional currency, common stock, basic, allowance, overtime/types, 
	[SystemCode]				NVARCHAR (255),
	[Memo]						NVARCHAR (2048), -- description
	[CustomsReference]			NVARCHAR (255), -- how it is referred to by Customs
	[UniversalProductCode]		NVARCHAR (255), -- for barcode readers
	[PreferredSupplierId]		INT,			-- FK, Table Agents, specially for purchasing
	-- The following properties are user-defined, used for reporting
	[ResourceLookup1Id]			INT,			-- UDL 
	[ResourceLookup2Id]			INT,			-- UDL 
	[ResourceLookup3Id]			INT,			-- UDL 
	[ResourceLookup4Id]			INT,			-- UDL 
	INDEX IX_ResourceList__Code ([Code])
);