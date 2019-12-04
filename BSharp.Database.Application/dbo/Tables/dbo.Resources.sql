-- Properties of a resource, if defined, must be compatible with those of the account used in it. They must also be compatibled with the entries used in it.
-- So, if a resource has a defined currency, it propagates to the entries used. The same applies if it has an external reference.
-- Similarly, if it has a defined monetary value, it propagates to the entries used
CREATE TABLE [dbo].[Resources] (
-- Resource can be seen as the true leaf level of Resource Classifications.
	[Id]							INT					CONSTRAINT [PK_Resources] PRIMARY KEY IDENTITY,
	[OperatingSegmentId]			INT					CONSTRAINT [FK_Resources__OperatingSegmentId] REFERENCES dbo.ResponsibilityCenters([Id]),
	[DefinitionId]					NVARCHAR (50)		NOT NULL,
	-- Inspired by IFRS, with additions to simplify application logic, to cater for user custom classifications for reporting purposes
	[ResourceClassificationId]		INT					NOT NULL,
	CONSTRAINT [FK_Resources__ResourceClassificationId_DefinitionId] FOREIGN KEY ([ResourceClassificationId], [DefinitionId]) REFERENCES [dbo].[ResourceClassifications] ([Id], [ResourceDefinitionId]),
	[Name]							NVARCHAR (255)		NOT NULL,
	CONSTRAINT [CK_Resources__ResourceDefinitionId_Name_DescriptorId] UNIQUE ([DefinitionId],[Name],[Identifier]),
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Identifier]					NVARCHAR (10), -- When not null, all measures (count, monetary value, mass, volumne, etc) must be fixed
	--[BatchCode]						NVARCHAR (10), -- when not null, measures need not be known per unit. 
	-- if inactive, it is not to be referenced in subsequent documents anymore.
	[IsActive]						BIT					NOT NULL DEFAULT 1,
	-- Unique within a definition - when no null - property that is language independent, --	Tag #, Coil #, Check #, LC #
	[Code]							NVARCHAR (255),

	[CountUnitId]					INT					CONSTRAINT [FK_Resources__CountUnitId] REFERENCES [dbo].[MeasurementUnits] ([Id]),
	[Count]							DECIMAL				DEFAULT 1,
	[CurrencyId]					NCHAR (3)			CONSTRAINT [FK_Resources__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
	[MonetaryValue]					DECIMAL, -- if [MonetaryValue] is not null, this value is forced in Entries
	[MassUnitId]					INT					CONSTRAINT [FK_Resources__MassUnitId] REFERENCES [dbo].[MeasurementUnits] ([Id]),
	[Mass]							DECIMAL,
	[VolumeUnitId]					INT					CONSTRAINT [FK_Resources__VolumeUnitId] REFERENCES [dbo].[MeasurementUnits] ([Id]),
	[Volume]						DECIMAL,
	[TimeUnitId]					INT					CONSTRAINT [FK_Resources__TimeUnitId] REFERENCES [dbo].[MeasurementUnits] ([Id]),
	[Time]							DECIMAL,

	[Description]					NVARCHAR (2048),
	[Description2]					NVARCHAR (2048),
	[Description3]					NVARCHAR (2048),
-- Google Drive, One Drive, etc. | Activate collaboration
	--[AttachmentsFolderURL]			NVARCHAR (255), 

	--[CustomsReference]				NVARCHAR (255), -- how it is referred to by Customs

	--[PreferredSupplierId]			INT,-- FK, Table Agents, specially for purchasing
	-- The following properties are user-defined, used for reporting
	[AvailableSince]				DATE, -- such as first availability date. makes sense with non-null descriptor
	[AvailableTill]					DATE, -- such as first discontinuity date
	--[UniqueReference1]				NVARCHAR(50), -- such as VIN, UPC, EPC, etc...
	--[UniqueReference2]				NVARCHAR(50), -- such as Engine number
	--[UniqueReference3]				NVARCHAR(50), -- such as Plate number
-- for financials
	--[AssetAccountId]				INT					CONSTRAINT [FK_Resources__AssetAccountId] FOREIGN KEY ([AssetAccountId]) REFERENCES [dbo].[Accounts] ([Id]),
	--[LiabilityAccountId]			INT					CONSTRAINT [FK_Resources__LiabilityAccountId] FOREIGN KEY ([LiabilityAccountId]) REFERENCES [dbo].[Accounts] ([Id]),
	--[EquityAccountId]				INT					CONSTRAINT [FK_Resources__EquityAccountId] FOREIGN KEY ([EquityAccountId]) REFERENCES [dbo].[Accounts] ([Id]),
	--[RevenueAccountId]				INT					CONSTRAINT [FK_Resources__RevenueAccountId] FOREIGN KEY ([RevenueAccountId]) REFERENCES [dbo].[Accounts] ([Id]),
	--[ExpensesAccountId]				INT					CONSTRAINT [FK_Resources__ExpensesAccountId] FOREIGN KEY ([ExpensesAccountId]) REFERENCES [dbo].[Accounts] ([Id]),

-- Example for services: Account manager and project manager
	--[Agent1Id]						INT					CONSTRAINT [FK_Resources__Agent1Id] FOREIGN KEY ([Agent1Id]) REFERENCES [dbo].[Agents] ([Id]),
	--[Agent2Id]						INT					CONSTRAINT [FK_Resources__Agent2Id] FOREIGN KEY ([Agent2Id]) REFERENCES [dbo].[Agents] ([Id]),
	--[Date1]							DATE,			-- Registration Date
	--[Date2]							DATE,			-- Oil change date
	--[Decimal1]						DECIMAL,
	--[Decimal2]						DECIMAL,
	--[INT1]							INT,			-- Engine Capacity
	--[INT2]							INT,
-- Examples for Steel finished goods are: Thickness and width. For cars: make and model.
	[Lookup1Id]						INT					CONSTRAINT [FK_Resources__Lookup1Id] REFERENCES [dbo].[Lookups] ([Id]),
	[Lookup2Id]						INT					CONSTRAINT [FK_Resources__Lookup2Id] REFERENCES [dbo].[Lookups] ([Id]),
	--[Lookup3Id]						INT					CONSTRAINT [FK_Resources__Lookup3Id] FOREIGN KEY ([Lookup3Id]) REFERENCES [dbo].[Lookups] ([Id]),
	--[Lookup4Id]						INT					CONSTRAINT [FK_Resources__Lookup4Id] FOREIGN KEY ([Lookup4Id]) REFERENCES [dbo].[Lookups] ([Id]),
	--[Lookup5Id]						INT					CONSTRAINT [FK_Resources__Lookup5Id] FOREIGN KEY ([Lookup5Id]) REFERENCES [dbo].[Lookups] ([Id]),
--	[Money1]						MONEY,
--	[Money2]						MONEY,
----  for additional information
--	[Text1]							NVARCHAR (255),
--	[Text2]							NVARCHAR (255), 
	--[State]					AS (CASE -- not sure about it. I though a legal case is a resource (service) but it is actually a job for costing
	--								WHEN [IsActive] = 1 THEN N'Active'
	--								WHEN [IsActive] = 1 AND [AvailableTill] IS NOT NULL THEN N'Error!'
	--								WHEN [IsActive] = 0 AND [AvailableTill] IS NULL THEN N'Dormant' -- frozen
	--								WHEN [IsActive] = 0 AND [AvailableTill] IS NOT NULL THEN N'Closed'
	--								ELSE NULL
	--							END) PERSISTED,
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Resources__DefinitionId_Name_DescriptorId]
  ON [dbo].[Resources]([DefinitionId], [Name], [Identifier]);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Resources__DefinitionId_Name2_DescriptorId]
  ON [dbo].[Resources]([DefinitionId], [Name2], [Identifier]) WHERE [Name2] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Resources__DefinitionId_Name3_DescriptorId]
  ON [dbo].[Resources]([DefinitionId], [Name3], [Identifier]) WHERE [Name3] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Resources__ResourceDefinitionId_Code]
  ON [dbo].[Resources]([DefinitionId], [Code]) WHERE [Code] IS NOT NULL;
GO

	-- The following three properties apply to the same three tables...
	-- LinkType between Document and Resource, Document and Agent, Agent and Resource
	-- [LinkedAgentsRelations] specifies RelatedAgentRelation with the resource, 
	-- [LinkedDocuments]
	-- [LinkedResources]