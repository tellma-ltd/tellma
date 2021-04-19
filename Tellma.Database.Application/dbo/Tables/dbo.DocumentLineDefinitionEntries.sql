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

	[RelationId]					INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__RelationId] REFERENCES dbo.[Relations]([Id]), 
	[RelationIsCommon]				BIT				NOT NULL DEFAULT 0,

	[CustodianId]					INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__CustodianId] REFERENCES dbo.[Relations]([Id]), 
	[CustodianIsCommon]				BIT				NOT NULL DEFAULT 0,
	[NotedRelationId]				INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__NotedRelationId] REFERENCES dbo.[Relations]([Id]), 
	[NotedRelationIsCommon]			BIT				NOT NULL DEFAULT 0,
	[ResourceId]					INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__ResourceId] REFERENCES dbo.[Resources]([Id]), 
	[ResourceIsCommon]				BIT				NOT NULL DEFAULT 0,

	[Quantity]						DECIMAL (19,4)	NULL,
	[QuantityIsCommon]				BIT				NOT NULL DEFAULT 0,
	[UnitId]						INT CONSTRAINT [FK_DocumentLineDefinitionEntries__UnitId] REFERENCES dbo.[Units]([Id]),
	[UnitIsCommon]					BIT				NOT NULL DEFAULT 0,

	[Time1]							DATETIME2 (2),
	[Time1IsCommon]					BIT				NOT NULL DEFAULT 0,
	[NotedDuration]					DECIMAL (19,4),
	[NotedDurationIsCommon]			BIT				NOT NULL DEFAULT 0,	
	[NotedUnitId]					INT				CONSTRAINT [FK_DocumentLineDefinitionEntries__NotedUnitId] REFERENCES [dbo].[Units] ([Id]),
	[NotedUnitIsCommon]				BIT				NOT NULL DEFAULT 0,
	[Time2]							DATETIME2 (2),
	[Time2IsCommon]					BIT				NOT NULL DEFAULT 0,

	[ExternalReference]				NVARCHAR (50), -- e.g., supplier invoice number, customer WT #
	[ExternalReferenceIsCommon]		BIT				NOT NULL DEFAULT 0,
	[ReferenceSourceId]				INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__ReferenceSourceId] REFERENCES dbo.[Relations]([Id]),
	[ReferenceSourceIsCommon]		BIT				NOT NULL DEFAULT 0,
	[InternalReference]				NVARCHAR (50), -- e.g., check number, customer invoice number
	[InternalReferenceIsCommon]		BIT				NOT NULL DEFAULT 0,


	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentLineDefinitionEntries__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentLineDefinitionEntries__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE INDEX [IX_DocumentLineDefinitionEntries__DocumentId] ON [dbo].[DocumentLineDefinitionEntries]([DocumentId]);
GO
CREATE INDEX [IX_DocumentLineDefinitionEntries__CreatedById] ON [dbo].[DocumentLineDefinitionEntries]([CreatedById]);
GO