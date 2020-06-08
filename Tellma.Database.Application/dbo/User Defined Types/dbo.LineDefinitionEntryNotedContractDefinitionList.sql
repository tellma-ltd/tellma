CREATE TYPE [dbo].[LineDefinitionEntryNotedContractDefinitionList] AS TABLE (
	[Index]						INT,
	[LineDefinitionEntryIndex]				INT,
	[LineDefinitionIndex]		INT,
	PRIMARY KEY ([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex]),
	[Id]						INT			DEFAULT 0,
	[NotedContractDefinitionId]		INT
);