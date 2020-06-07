CREATE TYPE [dbo].[LineDefinitionEntryResourceDefinitionList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT,
	[LineDefinitionIndex]		INT,
	PRIMARY KEY ([Index], [HeaderIndex], [LineDefinitionIndex]),
	[Id]						INT			DEFAULT 0,
	[ResourceDefinitionId]		INT
);