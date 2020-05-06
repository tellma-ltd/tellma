CREATE TYPE [dbo].[LineDefinitionEntryList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT		DEFAULT 0,
	[Direction]					SMALLINT,
	[AccountDefinitionId]		INT		NOT NULL,
	[EntryTypeId]				INT
);