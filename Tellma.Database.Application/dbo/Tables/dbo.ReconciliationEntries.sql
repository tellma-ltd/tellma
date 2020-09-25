CREATE TABLE [dbo].[ReconciliationEntries] (
	[EntryId]				INT					CONSTRAINT [PK_ReconciliationEntries] PRIMARY KEY,
	[ReconciliationId]		INT					NOT NULL CONSTRAINT [FK_ReconciliationEntries__ReconciliationId] REFERENCES dbo.Reconciliations([Id]) ON DELETE CASCADE
);
GO
CREATE INDEX [IX_ReconciliationEntries__ReconciliationId] ON [dbo].[ReconciliationEntries]([ReconciliationId]);
GO