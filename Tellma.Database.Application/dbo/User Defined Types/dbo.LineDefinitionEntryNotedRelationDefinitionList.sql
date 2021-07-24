CREATE TYPE [dbo].[LineDefinitionEntryNotedRelationDefinitionList] AS TABLE (
	[Index]						INT,
	[LineDefinitionEntryIndex]	INT,
	[LineDefinitionIndex]		INT,
	PRIMARY KEY ([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex]),
	[Id]						INT,
	[NotedRelationDefinitionId]	INT
);