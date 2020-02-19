CREATE TYPE [dbo].[AgentRateList] AS TABLE (
	[Index]				INT,
	[HeaderIndex]		INT		INDEX IX_AgentRateList__HeaderIndex ([HeaderIndex]),
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]				INT				NOT NULL DEFAULT 0,
	[ResourceId]		INT				NOT NULL,
	[UnitId]			INT				NOT NULL,
	[Rate]				DECIMAL (19,4)	NOT NULL CHECK ([Rate] >= 0),
	[CurrencyId]		NCHAR (3)		NOT NULL
);