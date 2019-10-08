CREATE TYPE [dbo].[ResourceList] AS TABLE (
	[Index]							INT					PRIMARY KEY,
	[Id]							INT					NOT NULL DEFAULT 0,
	[ResourceTypeId]				NVARCHAR (255)		NOT NULL,
	[ResourceClassificationId]		INT,
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (255),

	[AreaUnitId]					INT,
	--[UnitAreaMean]				DECIMAL,
	--[UnitAreaVariance]			DECIMAL (5,2),
	[CountUnitId]					INT,
	--[UnitCountMean]				DECIMAL,
	--[UnitCountVariance]			DECIMAL (5,2),
	[LengthUnitId]					INT,
	--[UnitLengthMean]				DECIMAL,
	--[UnitLengthVariance]			DECIMAL (5,2),
	[MassUnitId]					INT,
	--[UnitMassMean]				DECIMAL,
	--[UnitMassVariance]			DECIMAL (5,2),
	[MonetaryValueCurrencyId]		NCHAR (3),
	--[UnitMonetaryValueMean]		DECIMAL,
	--[UnitMonetaryValueVariance]	DECIMAL (5,2),
	[TimeUnitId]					INT,
	--[UnitTimeMean]				DECIMAL,
	--[UnitTimeVariance]			DECIMAL (5,2),
	[VolumeUnitId]					INT,
	--[UnitVolumeMean]				DECIMAL,
	--[UnitVolumeVariance]			DECIMAL (5,2),

	[Description]					NVARCHAR (2048),
	[Description2]					NVARCHAR (2048),
	[Description3]					NVARCHAR (2048),
	[CustomsReference]				NVARCHAR (255), -- how it is referred to by Customs

	--[PreferredSupplierId]			INT,			-- FK, Table Agents, specially for purchasing
	-- The following properties are user-defined, used for reporting
	--[Date1]							DATE,
	--[Date2]							DATE,
	[Lookup1Id]						INT,			-- UDL 
	[Lookup2Id]						INT,			-- UDL 
	[Lookup3Id]						INT,			-- UDL 
	[Lookup4Id]						INT,			-- UDL 
	INDEX IX_ResourceList__Code ([Code])
);