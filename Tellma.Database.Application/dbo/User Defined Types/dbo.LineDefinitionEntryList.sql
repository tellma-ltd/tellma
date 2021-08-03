CREATE TYPE [dbo].[LineDefinitionEntryList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[Direction]					SMALLINT,
	[ParentAccountTypeId]		INT,
	[EntryTypeId]				INT
);