CREATE TYPE [dbo].[LineDefinitionEntryRelationDefinitionList] AS TABLE (
	[Index]						INT,
	[LineDefinitionEntryIndex]	INT,
	[LineDefinitionIndex]		INT,
	PRIMARY KEY ([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex]),
	[Id]						INT,
	[RelationDefinitionId]		INT
);