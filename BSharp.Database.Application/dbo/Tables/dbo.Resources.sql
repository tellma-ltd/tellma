CREATE TABLE [dbo].[Resources] (
-- Resource can be seen as the true leaf level of Resource Classifications.
-- When isFungible = 0, transaction cannot be completed unless the Instance Id is specified.
	[Id]							INT					CONSTRAINT [PK_Resources] PRIMARY KEY IDENTITY,
	-- Inspired by IFRS, with additions to simplify application logic
	[ResourceTypeId]				NVARCHAR (255)		NOT NULL CONSTRAINT [FK_Resources__ResourceTypeId] FOREIGN KEY ([ResourceTypeId]) REFERENCES [dbo].[ResourceTypes] ([Id]),
	-- to define labels and control visibilities of dynamic properties
	[ResourceDefinitionId]			NVARCHAR (50)		NOT NULL CONSTRAINT [FK_Resources__ResourceDefinitionId] FOREIGN KEY ([ResourceDefinitionId]) REFERENCES [dbo].[ResourceDefinitions] ([Id]),	
	-- to cater for user custom classifications for reporting purposes
	[ResourceClassificationId]		INT					CONSTRAINT [FK_Resources__ResourceClassificationId] FOREIGN KEY ([ResourceClassificationId]) REFERENCES [dbo].[ResourceClassifications] ([Id]),
	[Name]							NVARCHAR (255)		NOT NULL CONSTRAINT [CK_Resources__ResourceDefinitionId_Name] UNIQUE ([ResourceDefinitionId],[Name]),
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	-- if inactive, it is not to be referenced in subsequent documents anymore.
	[IsActive]						BIT					NOT NULL DEFAULT 1,
	-- Unique within a definition - when no null - property that is language independent, --	Tag #, Coil #, Check #, LC #
	[Code]							NVARCHAR (255),

	-- Unit used to measure the area. This is either the physical area, or the area serviced, if service value is measureable by area (e.g., area of land irrigated)
	[AreaUnitId]					INT					CONSTRAINT [FK_Resources__AreaUnitId] FOREIGN KEY ([AreaUnitId]) REFERENCES [dbo].[MeasurementUnits] ([Id]),
	-- if resource is measureable primarily by mass or length or count, UnitAreaMean is the average per unit of mass or length or piece.
	-- If the resource is measureable primarily by Area  (الممسوحات), then UnitAreaMean = 1.
	[UnitAreaMean]					DECIMAL,
	-- If the resource is measureable primarily by Area  (الممسوحات), then UnitAreaVariance = 0
	[UnitAreaVariance]				DECIMAL (5,2)		DEFAULT 0,

	-- Unit used to measure the Count. This is either the physical Count, or the Count serviced, if service value is measureable by Count (e.g., number of copies made)
	[CountUnitId]					INT					CONSTRAINT [FK_Resources__CountUnitId] FOREIGN KEY ([CountUnitId]) REFERENCES [dbo].[MeasurementUnits] ([Id]),
	-- if resource is measureable primarily by mass or volume or area, UnitCountMean is the average per unit of area or mass or volume.
	-- If the resource is measureable primarily by Count  (المعدودات), then UnitCountMean = 1.
	[UnitCountMean]					DECIMAL,
	-- If the resource is measureable primarily by Count  (المعدودات), then UnitCountVariance = 0
	[UnitCountVariance]				DECIMAL (5,2),

	-- Unit used to measure the length. This is either the physical length, or the length serviced, if service value is measureable by length (e.g., distance of road travelled)
	[LengthUnitId]					INT					CONSTRAINT [FK_Resources__LengthUnitId] FOREIGN KEY ([LengthUnitId]) REFERENCES [dbo].[MeasurementUnits] ([Id]),
	-- if resource is measureable primarily by mass or volume or count, UnitLengthMean is the average per unit of mass or volume or piece.
	-- If the resource is measureable primarily by Length  (المذروعات), then UnitLengthMean = 1.
	[UnitLengthMean]				DECIMAL,
	-- If the resource is measureable primarily by Length  (المذروعات), then UnitLengthVariance = 0
	[UnitLengthVariance]			DECIMAL (5,2),

	-- Unit used to measure the mass. This is either the physical weight, or the weight serviced, if service value is measureable by weight (e.g., weight of items shipped)
	[MassUnitId]					INT					CONSTRAINT [FK_Resources__MassUnitId] FOREIGN KEY ([MassUnitId]) REFERENCES [dbo].[MeasurementUnits] ([Id]),
	-- if resource is measureable primarily by volume or length or count, UnitMassMean is the average mass per unit of volume or length or piece.
	-- If the resource is measureable primarily by mass (الموزونات), then UnitMassMean = 1.
	[UnitMassMean]					DECIMAL,
	-- if resource is measureable primarily volume or length or count, UnitMassMean is the maximum acceptable variation in mass from the mean, per unit of volume or length or piece.
	-- If the resource is measureable primarily by mass (الموزونات), then UnitMassVariance = 0.
	[UnitMassVariance]				DECIMAL (5,2),

	-- Currency used to measure the monetary value. This is either the resource monetary value, or the monetary amount serviced (e.g., money transferred)
	[MonetaryValueCurrencyId]		NCHAR (3)			NOT NULL DEFAULT CONVERT(NCHAR(3), SESSION_CONTEXT(N'FunctionalCurrencyId'))
														CONSTRAINT [FK_Resources__MValueCurrencyId] FOREIGN KEY ([MonetaryValueCurrencyId]) REFERENCES [dbo].[Currencies] ([Id]),
	-- if resource is measureable primarily by mass or volume or count, UnitValueMean is the average value per unit of mass or volume or piece.
	-- If the resource is measureable primarily by monetrary amount (النقود), then UnitValueMean = 1.
	[UnitMonetaryValueMean]			DECIMAL,
	-- If the resource is measureable primarily by monetary amount (النقود), then UniValueVariance = 0
	[UnitMonetaryValueVariance]		DECIMAL (5,2),

	-- Unit used to measure the time. Since time is intangible, this measure applies to services only (e.g., days worked)
	[TimeUnitId]					INT					CONSTRAINT [FK_Resources__TimeUnitId] FOREIGN KEY ([TimeUnitId]) REFERENCES [dbo].[MeasurementUnits] ([Id]),
	-- if service is measureable primarily by mass or volume or count, UnitTimeMean is the average time taken to process a unit of mass or volume or piece.
	-- If the service is measureable primarily by time (إجارة زمن), then UnitTimeMean = 1.
	[UnitTimeMean]					DECIMAL,
	-- If the service is measureable primarily by time (إجارة زمن), then UnitTimeVariance = 0
	[UnitTimeVariance]				DECIMAL (5,2),
		
	-- Unit used to measure the volume. This is either the physical volume, or the volumne serviced, if service value is measureable by volume (e.g., volume of water purified)
	[VolumeUnitId]					INT					CONSTRAINT [FK_Resources__VolumeUnitId] FOREIGN KEY ([VolumeUnitId]) REFERENCES [dbo].[MeasurementUnits] ([Id]),
	-- if resource is measureable primarily by mass or length or count, UnitVolumeMean is the average per unit of mass or length or piece.
	-- If the resource is measureable primarily by volumne  (المكيلات), then UnitVolumeMean = 1.
	[UnitVolumeMean]				DECIMAL,
	-- If the resource is measureable primarily by volumne  (المكيلات), then [UnitVolumeVariance] = 0.
	[UnitVolumeVariance]			DECIMAL (5,2),

	[Description]					NVARCHAR (2048),
	[Description2]					NVARCHAR (2048),
	[Description3]					NVARCHAR (2048),
-- Google Drive, One Drive, etc. | Activate collaboration
	[AttachmentsFolderURL]			NVARCHAR (255), 

	[CustomsReference]				NVARCHAR (255), -- how it is referred to by Customs

	[PreferredSupplierId]			INT,-- FK, Table Agents, specially for purchasing
	-- The following properties are user-defined, used for reporting
	[AvailableSince]				DATE, -- such as first availability date. 
	[AvailableTill]					DATE, -- such as first discontinuity date
	[UniqueReference1]				NVARCHAR(50), -- such as VIN, UPC, EPC, etc...
	[UniqueReference2]				NVARCHAR(50), -- such as Engine number
	[UniqueReference3]				NVARCHAR(50), -- such as Plate number
-- for financials
	[AssetAccountId]				INT					CONSTRAINT [FK_Resources__AssetAccountId] FOREIGN KEY ([AssetAccountId]) REFERENCES [dbo].[Accounts] ([Id]),
	[LiabilityAccountId]			INT					CONSTRAINT [FK_Resources__LiabilityAccountId] FOREIGN KEY ([LiabilityAccountId]) REFERENCES [dbo].[Accounts] ([Id]),
	[EquityAccountId]				INT					CONSTRAINT [FK_Resources__EquityAccountId] FOREIGN KEY ([EquityAccountId]) REFERENCES [dbo].[Accounts] ([Id]),
	[RevenueAccountId]				INT					CONSTRAINT [FK_Resources__RevenueAccountId] FOREIGN KEY ([RevenueAccountId]) REFERENCES [dbo].[Accounts] ([Id]),
	[ExpensesAccountId]				INT					CONSTRAINT [FK_Resources__ExpensesAccountId] FOREIGN KEY ([ExpensesAccountId]) REFERENCES [dbo].[Accounts] ([Id]),

-- Example for services: Account manager and project manager
	[Agent1Id]						INT					CONSTRAINT [FK_Resources__Agent1Id] FOREIGN KEY ([Agent1Id]) REFERENCES [dbo].[Agents] ([Id]),
	[Agent2Id]						INT					CONSTRAINT [FK_Resources__Agent2Id] FOREIGN KEY ([Agent2Id]) REFERENCES [dbo].[Agents] ([Id]),
	[Date1]							DATE,			-- Registration Date
	[Date2]							DATE,			-- Oil change date
	[Decimal1]						DECIMAL,
	[Decimal2]						DECIMAL,
	[INT1]							INT,			-- Engine Capacity
	[INT2]							INT,
-- Examples for Steel finished goods are: Thickness and width. For cars: make and model.
	[Lookup1Id]						INT					CONSTRAINT [FK_Resources__Lookup1Id] FOREIGN KEY ([Lookup1Id]) REFERENCES [dbo].[Lookups] ([Id]),
	[Lookup2Id]						INT					CONSTRAINT [FK_Resources__Lookup2Id] FOREIGN KEY ([Lookup2Id]) REFERENCES [dbo].[Lookups] ([Id]),
	[Lookup3Id]						INT					CONSTRAINT [FK_Resources__Lookup3Id] FOREIGN KEY ([Lookup3Id]) REFERENCES [dbo].[Lookups] ([Id]),
	[Lookup4Id]						INT					CONSTRAINT [FK_Resources__Lookup4Id] FOREIGN KEY ([Lookup4Id]) REFERENCES [dbo].[Lookups] ([Id]),
	[Lookup5Id]						INT					CONSTRAINT [FK_Resources__Lookup5Id] FOREIGN KEY ([Lookup5Id]) REFERENCES [dbo].[Lookups] ([Id]),
	[Money1]						MONEY,
	[Money2]						MONEY,
--  for additional information
	[Text1]							NVARCHAR (255),
	[Text2]							NVARCHAR (255), 
	[State]					AS (CASE
									WHEN [IsActive] = 1 THEN N'Active'
									WHEN [IsActive] = 1 AND [AvailableTill] IS NOT NULL THEN N'Error!'
									WHEN [IsActive] = 0 AND [AvailableTill] IS NULL THEN N'Dormant' -- frozen
									WHEN [IsActive] = 0 AND [AvailableTill] IS NOT NULL THEN N'Closed'
									ELSE NULL
								END) PERSISTED,
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
);
GO
--CREATE UNIQUE NONCLUSTERED INDEX [IX_Resources__ResourceDefinitionId_Name]
--  ON [dbo].[Resources]([ResourceDefinitionId], [Name]);
--GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Resources__ResourceDefinitionId_Name2]
  ON [dbo].[Resources]([ResourceDefinitionId], [Name2]) WHERE [Name2] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Resources__ResourceDefinitionId_Name3]
  ON [dbo].[Resources]([ResourceDefinitionId], [Name3]) WHERE [Name3] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Resources__ResourceDefinitionId_Code]
  ON [dbo].[Resources]([ResourceDefinitionId], [Code]) WHERE [Code] IS NOT NULL;
GO

	-- The following three properties apply to the same three tables...
	-- LinkType between Document and Resource, Document and Agent, Agent and Resource
	-- [LinkedAgentsRelations] specifies RelatedAgentRelation with the resource, 
	-- [LinkedDocuments]
	-- [LinkedResources]