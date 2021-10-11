CREATE TYPE [dbo].[AccountTypeNotedResourceDefinitionList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT			NOT NULL DEFAULT 0,
	[NotedResourceDefinitionId]	INT
);