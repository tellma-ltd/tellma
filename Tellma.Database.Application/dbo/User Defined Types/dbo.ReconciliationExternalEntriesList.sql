CREATE TYPE [dbo].ReconciliationExternalEntriesList AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			NOT NULL DEFAULT 0 INDEX IX_ReconciliationExternalEntriesList_HeaderIndex ([HeaderIndex]),
	PRIMARY KEY ([Index], [HeaderIndex]),
	[ExternalEntryIndex]		INT,
	[ExternalEntryId]			INT
);