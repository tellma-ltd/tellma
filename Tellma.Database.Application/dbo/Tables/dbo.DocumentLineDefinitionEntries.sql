CREATE TABLE [dbo].[DocumentLineDefinitionEntries] (
	[Id]							INT				CONSTRAINT [PK_DocumentLineDefinitionEntries] PRIMARY KEY IDENTITY,
	[DocumentId]					INT				NOT NULL CONSTRAINT [FK_DocumentLineDefinitionEntries__DocumentId] REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	[LineDefinitionId]				INT				NOT NULL CONSTRAINT [FK_DocumentLineDefinitionEntries__LineDefinitionId] REFERENCES [dbo].[LineDefinitions] ([Id]),

	-- Additional properties to simplify data entry. No report should be based on them!!!
	[EntryIndex]					INT,
	-- always with EntryIndex = 0
	[PostingDate]					DATE, 
	[PostingDateIsCommon]			BIT				NOT NULL DEFAULT 1,
	[Memo]							NVARCHAR (255),
	[MemoIsCommon]					BIT				NOT NULL DEFAULT 1,
	-- With any entry Index

	[CurrencyId]					NCHAR (3) CONSTRAINT [FK_DocumentLineDefinitionEntries__CurrencyId] REFERENCES dbo.[Currencies]([Id]),
	[CurrencyIsCommon]				BIT				NOT NULL DEFAULT 0,	
	[CenterId]						INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__CenterId] REFERENCES dbo.[Centers]([Id]), 
	[CenterIsCommon]				BIT				NOT NULL DEFAULT 0,

	[AgentId]						INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__AgentId] REFERENCES dbo.[Agents]([Id]), 
	[AgentIsCommon]				BIT				NOT NULL DEFAULT 0,

	[NotedAgentId]					INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__NotedAgentId] REFERENCES dbo.[Agents]([Id]), 
	[NotedAgentIsCommon]			BIT				NOT NULL DEFAULT 0,
	[ResourceId]					INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__ResourceId] REFERENCES dbo.[Resources]([Id]), 
	[ResourceIsCommon]				BIT				NOT NULL DEFAULT 0,
	[NotedResourceId]				INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__NotedResourceId] REFERENCES dbo.[Resources]([Id]), 
	[NotedResourceIsCommon]			BIT				NOT NULL DEFAULT 0,

	[Quantity]						DECIMAL (19,4)	NULL,
	[QuantityIsCommon]				BIT				NOT NULL DEFAULT 0,
	[UnitId]						INT CONSTRAINT [FK_DocumentLineDefinitionEntries__UnitId] REFERENCES dbo.[Units]([Id]),
	[UnitIsCommon]					BIT				NOT NULL DEFAULT 0,

	[Time1]							DATETIME2 (2),
	[Time1IsCommon]					BIT				NOT NULL DEFAULT 0,
	[Duration]						DECIMAL (19,4),
	[DurationIsCommon]				BIT				NOT NULL DEFAULT 0,	
	[DurationUnitId]				INT				CONSTRAINT [FK_DocumentLineDefinitionEntries__DurationUnitId] REFERENCES [dbo].[Units] ([Id]),
	[DurationUnitIsCommon]			BIT				NOT NULL DEFAULT 0,
	[Time2]							DATETIME2 (2),
	[Time2IsCommon]					BIT				NOT NULL DEFAULT 0,

	[ExternalReference]				NVARCHAR (50), -- e.g., supplier invoice number, customer WT #
	[ExternalReferenceIsCommon]		BIT				NOT NULL DEFAULT 0,
	[ReferenceSourceId]				INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__ReferenceSourceId] REFERENCES dbo.[Agents]([Id]),
	[ReferenceSourceIsCommon]		BIT				NOT NULL DEFAULT 0,
	[InternalReference]				NVARCHAR (50), -- e.g., check number, customer invoice number
	[InternalReferenceIsCommon]		BIT				NOT NULL DEFAULT 0,


	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL CONSTRAINT [FK_DocumentLineDefinitionEntries__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL CONSTRAINT [FK_DocumentLineDefinitionEntries__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE UNIQUE INDEX [UQ_DocumentLineDefinitionEntries__DocumentId_LineDefinitionId_EntryIndex]
	ON [dbo].[DocumentLineDefinitionEntries]([DocumentId], [LineDefinitionId], [EntryIndex]);
GO
CREATE INDEX [IX_DocumentLineDefinitionEntries__CreatedById] ON [dbo].[DocumentLineDefinitionEntries]([CreatedById]);
GO