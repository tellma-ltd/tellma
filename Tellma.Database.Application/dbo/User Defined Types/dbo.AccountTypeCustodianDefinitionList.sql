CREATE TYPE [dbo].[AccountTypeCustodianDefinitionList] AS TABLE (
	[Index]						INT,
	[HeaderIndex]				INT			DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT			DEFAULT 0,
	[CustodianDefinitionId]		INT NOT NULL
);