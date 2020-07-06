CREATE TYPE [dbo].[ResourceUnitList] AS TABLE
(
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT			NOT NULL DEFAULT 0,
	[UnitId]					INT			NOT NULL
)