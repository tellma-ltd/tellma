CREATE TYPE [dbo].[DocumentDefinitionLineDefinitionList] AS TABLE
(
	[Index]					INT,
	[HeaderIndex]			INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]					INT				NOT NULL DEFAULT 0,
	[LineDefinitionId]		INT,
	UNIQUE ([HeaderIndex], [LineDefinitionId]),
	[IsVisibleByDefault]	BIT
);