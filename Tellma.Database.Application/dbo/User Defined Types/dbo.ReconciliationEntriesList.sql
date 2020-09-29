CREATE TYPE [dbo].ReconciliationEntriesList AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			NOT NULL DEFAULT 0 INDEX IX_ReconciliationEntriesList_HeaderIndex ([HeaderIndex]),
	PRIMARY KEY ([Index], [HeaderIndex]),
	[EntryId]					INT
);