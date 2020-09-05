CREATE TYPE [dbo].[DocumentLineDefinitionEntryList] AS TABLE
(
	[Index]					INT		DEFAULT 0,
	[DocumentIndex]			INT		DEFAULT 0,
	PRIMARY KEY ([Index], [DocumentIndex]),
	[Id]					INT		DEFAULT 0,
	[LineDefinitionId]		INT,
	UNIQUE ([DocumentIndex], [LineDefinitionId]),

	[EntryIndex]					INT,
	[PostingDate]					DATE, 
	[PostingDateIsCommon]			BIT				NOT NULL DEFAULT 1,
	[Memo]							NVARCHAR (255),
	[MemoIsCommon]					BIT				NOT NULL DEFAULT 1,
	
	[NotedRelationId]				INT,
	[NotedRelationIsCommon]			BIT				NOT NULL DEFAULT 0,

	[CurrencyId]					NCHAR (3),
	[CurrencyIsCommon]				BIT				NOT NULL DEFAULT 0,

	[CustodyId]						INT,
	[CustodyIsCommon]				BIT				NOT NULL DEFAULT 0,
	[ResourceId]					INT,
	[ResourceIsCommon]				BIT				NOT NULL DEFAULT 0,
	[Quantity]						DECIMAL (19,4)	NULL,
	[QuantityIsCommon]				BIT				NOT NULL DEFAULT 0,
	[UnitId]						INT,
	[UnitIsCommon]					BIT				NOT NULL DEFAULT 0,

	[CenterId]						INT,
	[CenterIsCommon]				BIT				NOT NULL DEFAULT 0,

	[Time1]							DATETIME2 (2),
	[Time1IsCommon]					BIT				NOT NULL DEFAULT 0,
	[Time2]							DATETIME2 (2),
	[Time2IsCommon]					BIT				NOT NULL DEFAULT 0,

	[ExternalReference]				NVARCHAR (50), -- e.g., invoice number
	[ExternalReferenceIsCommon]		BIT				NOT NULL DEFAULT 0,

	[AdditionalReference]			NVARCHAR (50), -- e.g., machine number
	[AdditionalReferenceIsCommon]	BIT				NOT NULL DEFAULT 0
);