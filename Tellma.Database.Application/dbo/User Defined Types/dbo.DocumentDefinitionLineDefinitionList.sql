CREATE TYPE [dbo].[DocumentDefinitionLineDefinitionList] AS TABLE
(
	[Index]					INT		DEFAULT 0,
	[HeaderIndex]			INT		DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]					INT		DEFAULT 0,
	[LineDefinitionId]		INT,
	UNIQUE ([HeaderIndex], [LineDefinitionId]),
	[IsVisibleByDefault]	BIT
);