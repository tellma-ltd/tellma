CREATE TYPE [dbo].[AccountTypeNotedContractDefinitionList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT			DEFAULT 0,
	[NotedContractDefinitionId]		INT NOT NULL
);