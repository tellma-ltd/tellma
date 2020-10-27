CREATE TYPE [dbo].ReconciliationEntryList AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			NOT NULL DEFAULT 0 INDEX IX_ReconciliationEntryList_HeaderIndex ([HeaderIndex]),
	PRIMARY KEY ([Index], [HeaderIndex]),
	[EntryId]					INT
);