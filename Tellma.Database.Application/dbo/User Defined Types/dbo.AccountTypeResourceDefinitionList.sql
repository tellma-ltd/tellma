CREATE TYPE [dbo].[AccountTypeResourceDefinitionList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT,
	[ResourceDefinitionId]		INT
);