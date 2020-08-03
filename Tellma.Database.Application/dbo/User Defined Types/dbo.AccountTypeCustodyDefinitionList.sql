CREATE TYPE [dbo].[AccountTypeCustodyDefinitionList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT			DEFAULT 0,
	[CustodyDefinitionId]		INT NOT NULL
);