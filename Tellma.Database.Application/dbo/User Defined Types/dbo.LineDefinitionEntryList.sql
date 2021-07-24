CREATE TYPE [dbo].[LineDefinitionEntryList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT,
	[Direction]					SMALLINT,
	[ParentAccountTypeId]		INT,
	[EntryTypeId]				INT
);