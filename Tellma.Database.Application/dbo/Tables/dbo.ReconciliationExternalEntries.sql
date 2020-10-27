CREATE TABLE [dbo].[ReconciliationExternalEntries] (
	[ExternalEntryId]		INT					CONSTRAINT [PK_ReconciliationExternalEntries] PRIMARY KEY,
	[ReconciliationId]		INT					NOT NULL CONSTRAINT [FK_ReconciliationExternalEntries__ReconciliationId] REFERENCES dbo.Reconciliations([Id]) ON DELETE CASCADE
);
GO
CREATE INDEX [IX_ReconciliationExternalEntries__ReconciliationId] ON [dbo].[ReconciliationExternalEntries]([ReconciliationId]);
GO