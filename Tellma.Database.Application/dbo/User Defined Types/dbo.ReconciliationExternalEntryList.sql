CREATE TYPE [dbo].ReconciliationExternalEntryList AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			NOT NULL DEFAULT 0 INDEX IX_ReconciliationExternalEntryList_HeaderIndex ([HeaderIndex]),
	PRIMARY KEY ([Index], [HeaderIndex]),
	[ExternalEntryIndex]		INT,
	[ExternalEntryId]			INT
);