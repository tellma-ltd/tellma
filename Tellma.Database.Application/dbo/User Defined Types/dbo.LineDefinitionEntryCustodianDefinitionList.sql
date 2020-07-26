CREATE TYPE [dbo].[LineDefinitionEntryCustodianDefinitionList] AS TABLE (
	[Index]						INT,
	[LineDefinitionEntryIndex]	INT,
	[LineDefinitionIndex]		INT,
	PRIMARY KEY ([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex]),
	[Id]						INT			DEFAULT 0,
	[CustodianDefinitionId]		INT
);