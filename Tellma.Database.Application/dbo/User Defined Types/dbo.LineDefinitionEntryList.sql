CREATE TYPE [dbo].[LineDefinitionEntryList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			NOT NULL DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT			NOT NULL DEFAULT 0,
	[Direction]					SMALLINT,
	[AccountTypeId]				INT NOT NULL,
	[EntryTypeId]				INT
);