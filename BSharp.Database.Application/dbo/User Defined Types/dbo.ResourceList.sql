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
	[MonetaryValueCurrencyId]		NCHAR (3) DEFAULT CONVERT(NCHAR(3), SESSION_CONTEXT(N'FunctionalCurrencyId')),
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
	[AttachmentsFolderURL]			NVARCHAR (255), -- Comment

	[CustomsReference]				NVARCHAR (255), -- how it is referred to by Customs
	--[PreferredSupplierId]			INT,			-- FK, Table Agents, specially for purchasing

	[AvailableSince]				DATE,			-- Comment
	[AvailableTill]					DATE,			-- Comment
	[GloballyUniqueReference]		NVARCHAR(50),	-- Comment
	
	[AssetAccountId]				INT,			-- Comment
	[LiabilityAccountId]			INT,			-- Comment
	[EquityAccountId]				INT,			-- Comment
	[RevenueAccountId]				INT,			-- Comment
	[ExpensesAccountId]				INT,			-- Comment

	[Agent1Id]						INT,			-- Comment
	[Agent2Id]						INT,			-- Comment
	[Date1]							DATE,			-- Comment
	[Date2]							DATE,			-- Comment

	[Lookup1Id]						INT,
	[Lookup2Id]						INT,
	[Lookup3Id]						INT,
	[Lookup4Id]						INT,
	[Lookup5Id]						INT,			-- Comment
	[Text1]							NVARCHAR (255), -- Comment
	[Text2]							NVARCHAR (255), -- Comment

	INDEX IX_ResourceList__Code ([Code])
);