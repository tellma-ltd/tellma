CREATE TYPE [dbo].[DocumentLineDefinitionEntryList] AS TABLE
(
	[Index]							INT		DEFAULT 0,
	[DocumentIndex]					INT		DEFAULT 0,
	PRIMARY KEY ([Index], [DocumentIndex]),
	[Id]							INT		DEFAULT 0,
	[LineDefinitionId]				INT,
	[EntryIndex]					INT,
	UNIQUE ([DocumentIndex], [LineDefinitionId], [EntryIndex]),

	[PostingDate]					DATE, 
	[PostingDateIsCommon]			BIT				NOT NULL DEFAULT 1,
	[Memo]							NVARCHAR (255),
	[MemoIsCommon]					BIT				NOT NULL DEFAULT 1,

	[CurrencyId]					NCHAR (3),
	[CurrencyIsCommon]				BIT				NOT NULL DEFAULT 0,
	[CenterId]						INT,
	[CenterIsCommon]				BIT				NOT NULL DEFAULT 0,
	
	[CustodianId]					INT,
	[CustodianIsCommon]				BIT				NOT NULL DEFAULT 0,
	[CustodyId]						INT,
	[CustodyIsCommon]				BIT				NOT NULL DEFAULT 0,
	[ParticipantId]					INT,
	[ParticipantIsCommon]			BIT				NOT NULL DEFAULT 0,
	[ResourceId]					INT,
	[ResourceIsCommon]				BIT				NOT NULL DEFAULT 0,

	[Quantity]						DECIMAL (19,4)	NULL,
	[QuantityIsCommon]				BIT				NOT NULL DEFAULT 0,
	[UnitId]						INT,
	[UnitIsCommon]					BIT				NOT NULL DEFAULT 0,
	[Time1]							DATETIME2 (2),
	[Time1IsCommon]					BIT				NOT NULL DEFAULT 0,
	[Time2]							DATETIME2 (2),
	[Time2IsCommon]					BIT				NOT NULL DEFAULT 0,

	[ExternalReference]				NVARCHAR (50),
	[ExternalReferenceIsCommon]		BIT				NOT NULL DEFAULT 0,
	[InternalReference]				NVARCHAR (50),
	[InternalReferenceIsCommon]		BIT				NOT NULL DEFAULT 0
);