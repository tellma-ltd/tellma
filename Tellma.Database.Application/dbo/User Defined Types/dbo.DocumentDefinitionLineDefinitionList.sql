CREATE TYPE [dbo].[DocumentDefinitionLineDefinitionList] AS TABLE
(
	[Index]					INT,
	[HeaderIndex]			INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]					INT,
	[LineDefinitionId]		INT,
	UNIQUE ([HeaderIndex], [LineDefinitionId]),
	[IsVisibleByDefault]	BIT
);