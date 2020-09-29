CREATE TYPE [dbo].ExternalEntryList AS TABLE (
	[Index]						INT					PRIMARY KEY,
	[Id]						INT					NOT NULL DEFAULT 0,
	[PostingDate]				DATE,
	[Direction]					SMALLINT,
	[MonetaryValue]				DECIMAL (19,4),
	[ExternalReference]			NVARCHAR (50)
);