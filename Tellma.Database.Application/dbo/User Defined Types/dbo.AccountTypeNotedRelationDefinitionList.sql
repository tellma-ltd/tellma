CREATE TYPE [dbo].[AccountTypeNotedRelationDefinitionList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT			DEFAULT 0,
	[NotedRelationDefinitionId]		INT NOT NULL
);