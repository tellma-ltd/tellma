CREATE TYPE [dbo].[AccountTypeRelationDefinitionList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT			DEFAULT 0,
	[RelationDefinitionId]		INT NOT NULL
);