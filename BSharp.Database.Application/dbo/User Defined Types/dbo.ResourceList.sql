CREATE TYPE [dbo].[ResourceList] AS TABLE (
	[Index]							INT					PRIMARY KEY,
	[Id]							INT					NOT NULL DEFAULT 0,
	[ResourceTypeId]				NVARCHAR (255)		NOT NULL,
	[ResourceClassificationId]		INT,
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (255),

	[CountUnitId]					INT,

	[CurrencyId]					NCHAR (3),
	[MonetaryValue]					DECIMAL,
	[MassUnitId]					INT,
	[VolumeUnitId]					INT,
	[TimeUnitId]					INT,

	--[Mass]							DECIMAL,
	--[Volume]						DECIMAL,
	--[Time]							DECIMAL,

	[Description]					NVARCHAR (2048),
	[Description2]					NVARCHAR (2048),
	[Description3]					NVARCHAR (2048),
	[AttachmentsFolderURL]			NVARCHAR (255), -- Comment

	[CustomsReference]				NVARCHAR (255), -- how it is referred to by Customs
	--[PreferredSupplierId]			INT,			-- FK, Table Agents, specially for purchasing

	[AvailableSince]				DATE,			-- Comment
	[AvailableTill]					DATE,			-- Comment

	[UniqueReference1]				NVARCHAR(50), -- such as VIN, UPC, EPC, etc...
	[UniqueReference2]				NVARCHAR(50), -- such as Engine number
	[UniqueReference3]				NVARCHAR(50), -- such as Plate number
	
	[AssetAccountId]				INT,			-- Comment
	[LiabilityAccountId]			INT,			-- Comment
	[EquityAccountId]				INT,			-- Comment
	[RevenueAccountId]				INT,			-- Comment
	[ExpensesAccountId]				INT,			-- Comment

	[Agent1Id]						INT,			-- Comment
	[Agent2Id]						INT,			-- Comment
	[Date1]							DATE,			-- Comment
	[Date2]							DATE,			-- Comment
	[Decimal1]						DECIMAL,
	[Decimal2]						DECIMAL,
	[INT1]							INT,			-- Engine Capacity
	[INT2]							INT,

	[Lookup1Id]						INT,
	[Lookup2Id]						INT,
	[Lookup3Id]						INT,
	[Lookup4Id]						INT,
	[Lookup5Id]						INT,			-- Comment
	[Money1]						MONEY,
	[Money2]						MONEY,
	[Text1]							NVARCHAR (255), -- Comment
	[Text2]							NVARCHAR (255), -- Comment

	INDEX IX_ResourceList__Code ([Code])
);