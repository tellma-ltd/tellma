CREATE TYPE [dbo].[ResourceUnitList] AS TABLE
(
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT,
	[UnitId]					INT
)