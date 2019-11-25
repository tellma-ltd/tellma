CREATE TYPE [dbo].[ResourceList] AS TABLE (
	[Index]							INT					PRIMARY KEY,
	[Id]							INT					NOT NULL DEFAULT 0,
	[OperatingSegmentId]			INT,
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
	[AttachmentsFolderURL]			NVARCHAR (255), 

	[CustomsReference]				NVARCHAR (255), -- how it is referred to by Customs
	--[PreferredSupplierId]			INT,			-- FK, Table Agents, specially for purchasing

	[AvailableSince]				DATE,			
	[AvailableTill]					DATE,			

	[UniqueReference1]				NVARCHAR(50), -- such as VIN, UPC, EPC, etc...
	[UniqueReference2]				NVARCHAR(50), -- such as Engine number
	[UniqueReference3]				NVARCHAR(50), -- such as Plate number
	
	[AssetAccountId]				INT,			
	[LiabilityAccountId]			INT,			
	[EquityAccountId]				INT,			
	[RevenueAccountId]				INT,			
	[ExpensesAccountId]				INT,			

	[Agent1Id]						INT,			
	[Agent2Id]						INT,			
	[Date1]							DATE,			
	[Date2]							DATE,			
	[Decimal1]						DECIMAL,
	[Decimal2]						DECIMAL,
	[INT1]							INT,			-- Engine Capacity
	[INT2]							INT,

	[Lookup1Id]						INT,
	[Lookup2Id]						INT,
	[Lookup3Id]						INT,
	[Lookup4Id]						INT,
	[Lookup5Id]						INT,			
	[Money1]						MONEY,
	[Money2]						MONEY,
	[Text1]							NVARCHAR (255), 
	[Text2]							NVARCHAR (255), 

	INDEX IX_ResourceList__Code ([Code])
);