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
	[ParticipantId]					INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__ParticipantId] REFERENCES dbo.[Relations]([Id]), 
	[ParticipantIsCommon]			BIT				NOT NULL DEFAULT 0,

	[CurrencyId]					NCHAR (3) CONSTRAINT [FK_DocumentLineDefinitionEntries__CurrencyId] REFERENCES dbo.Currencies([Id]),
	[CurrencyIsCommon]				BIT				NOT NULL DEFAULT 0,

	[CustodyId]						INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__CustodyId] REFERENCES dbo.[Custodies]([Id]), 
	[CustodyIsCommon]				BIT				NOT NULL DEFAULT 0,
	[ResourceId]					INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__DebitResourceId] REFERENCES dbo.[Resources]([Id]), 
	[ResourceIsCommon]				BIT				NOT NULL DEFAULT 0,
	[Quantity]						DECIMAL (19,4)	NULL,
	[QuantityIsCommon]				BIT				NOT NULL DEFAULT 0,
	[UnitId]						INT CONSTRAINT [FK_DocumentLineDefinitionEntries__UnitId] REFERENCES dbo.[Units]([Id]),
	[UnitIsCommon]					BIT				NOT NULL DEFAULT 0,

	[CenterId]						INT	CONSTRAINT [FK_DocumentLineDefinitionEntries__CenterId] REFERENCES dbo.[Centers]([Id]), 
	[CenterIsCommon]				BIT				NOT NULL DEFAULT 0,

	[Time1]							DATETIME2 (2),
	[Time1IsCommon]					BIT				NOT NULL DEFAULT 0,
	[Time2]							DATETIME2 (2),
	[Time2IsCommon]					BIT				NOT NULL DEFAULT 0,

	[ExternalReference]				NVARCHAR (50), -- e.g., invoice number
	[ExternalReferenceIsCommon]		BIT				NOT NULL DEFAULT 0,

	[AdditionalReference]			NVARCHAR (50), -- e.g., machine number
	[AdditionalReferenceIsCommon]	BIT				NOT NULL DEFAULT 0,


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