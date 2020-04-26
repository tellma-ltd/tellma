CREATE TYPE [dbo].[LineList] AS TABLE (
	[Index]						INT,
	[DocumentIndex]				INT		INDEX IX_LineList_DocumentIndex ([DocumentIndex]),
	PRIMARY KEY ([Index], [DocumentIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[DefinitionId]				INT				NOT NULL,
	--[AgentId]					INT,
	--[ResourceId]				INT,
	--[CurrencyId]				NCHAR (3),
	--[MonetaryValue]			DECIMAL (19,4),--			NOT NULL DEFAULT 0,
	--[Quantity]				DECIMAL (19,4),
	--[UnitId]					INT,
	--[Value]					DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- equivalent in functional currency
	[Memo]						NVARCHAR (255) -- a textual description for statements and reports
);