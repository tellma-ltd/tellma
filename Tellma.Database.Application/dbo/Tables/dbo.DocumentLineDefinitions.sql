CREATE TABLE [dbo].[DocumentLineDefinitions] (
	[Id]							INT				CONSTRAINT [PK_DocumentLineDefinitions] PRIMARY KEY IDENTITY,
	[DocumentId]					INT				NOT NULL CONSTRAINT [FK_DocumentLineDefinitions__DocumentId] REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	[LineDefinitionId]				INT				NOT NULL CONSTRAINT [FK_DocumentLineDefinitions__LineDefinitionId] REFERENCES [dbo].[LineDefinitions] ([Id]),

	-- Additional properties to simplify data entry. No report should be based on them!!!
	[PostingDate]					DATE,
	[PostingDateIsCommon]			DATE,

	[Memo]							NVARCHAR (255),
	[MemoIsCommon]					BIT				NOT NULL DEFAULT 1,

	[NotedRelationId]				INT	CONSTRAINT [FK_DocumentLineDefinitions__NotedRelationId] REFERENCES dbo.[Relations]([Id]), 
	[NotedRelationIsCommon]			BIT				NOT NULL DEFAULT 0,

	[CurrencyId]					NCHAR (3) CONSTRAINT [FK_DocumentLineDefinitions__CurrencyId] REFERENCES dbo.Currencies([Id]),
	[CurrencyIsCommon]				BIT				NOT NULL DEFAULT 0,

	[ExternalReference]				NVARCHAR (50), -- e.g., invoice number
	[ExternalReferenceIsCommon]		BIT				NOT NULL DEFAULT 0,

	[AdditionalReference]			NVARCHAR (50), -- e.g., machine number
	[AdditionalReferenceIsCommon]	BIT				NOT NULL DEFAULT 0,

	-- Tab specific, do not appear in header
	[DebitCustodyId]				INT	CONSTRAINT [FK_DocumentLineDefinitions__DebitCustodyId] REFERENCES dbo.[Custodies]([Id]), 
	[DebitCustodyIsCommon]			BIT				NOT NULL DEFAULT 0,
	[CreditCustodyId]				INT	CONSTRAINT [FK_DocumentLineDefinitions__CreditCustodyId] REFERENCES dbo.[Custodies]([Id]), 
	[CreditCustodyIsCommon]			BIT				NOT NULL DEFAULT 0,

	[DebitResourceId]				INT	CONSTRAINT [FK_DocumentLineDefinitions__DebitResourceId] REFERENCES dbo.[Resources]([Id]), 
	[DebitResourceIsCommon]			BIT				NOT NULL DEFAULT 0,
	[CreditResourceId]				INT	CONSTRAINT [FK_DocumentLineDefinitions__CreditResourceId] REFERENCES dbo.[Resources]([Id]), 
	[CreditResourceIsCommon]		BIT				NOT NULL DEFAULT 0,	

	[Quantity]						DECIMAL (19,4)	NULL,
	[QuantityIsCommon]				BIT				NOT NULL DEFAULT 0,
	[UnitId]						INT CONSTRAINT [FK_DocumentLineDefinitions__UnitId] REFERENCES dbo.[Units]([Id]),
	[UnitIsCommon]					BIT				NOT NULL DEFAULT 0,

	[CenterId]						INT	CONSTRAINT [FK_DocumentLineDefinitions__CenterId] REFERENCES dbo.[Centers]([Id]), 
	[CenterIsCommon]				BIT				NOT NULL DEFAULT 0,

	[Time1]							DATETIME2 (2),
	[Time1IsCommon]					BIT				NOT NULL DEFAULT 0,
	[Time2]							DATETIME2 (2),
	[Time2IsCommon]					BIT				NOT NULL DEFAULT 0,


	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentLineDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE INDEX [IX_DocumentLineDefinitions__DocumentId] ON [dbo].[DocumentLineDefinitions]([DocumentId]);
GO
CREATE INDEX [IX_DocumentLineDefinitions__CreatedById] ON [dbo].[DocumentLineDefinitions]([CreatedById]);
GO