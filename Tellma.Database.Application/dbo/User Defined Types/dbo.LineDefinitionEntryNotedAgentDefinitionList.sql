CREATE TYPE [dbo].[LineDefinitionEntryNotedAgentDefinitionList] AS TABLE (
	[Index]						INT,
	[LineDefinitionEntryIndex]	INT,
	[LineDefinitionIndex]		INT,
	PRIMARY KEY ([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[NotedAgentDefinitionId]	INT
);