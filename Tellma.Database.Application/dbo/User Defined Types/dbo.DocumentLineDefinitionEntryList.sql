CREATE TYPE [dbo].[DocumentLineDefinitionEntryList] AS TABLE
(
	[Index]							INT		DEFAULT 0,
	[DocumentIndex]					INT		DEFAULT 0,
	PRIMARY KEY ([Index], [DocumentIndex]),
	[Id]							INT		NOT NULL DEFAULT 0,
	[LineDefinitionId]				INT,
	[EntryIndex]					INT,
	UNIQUE ([DocumentIndex], [LineDefinitionId], [EntryIndex]),

	[PostingDate]					DATE, 
	[PostingDateIsCommon]			BIT,
	[Memo]							NVARCHAR (255),
	[MemoIsCommon]					BIT,

	[CurrencyId]					NCHAR (3),
	[CurrencyIsCommon]				BIT,
	[CenterId]						INT,
	[CenterIsCommon]				BIT,

	[AgentId]						INT,
	[AgentIsCommon]					BIT,	

	[NotedAgentId]					INT,
	[NotedAgentIsCommon]			BIT,
	[ResourceId]					INT,
	[ResourceIsCommon]				BIT,

	[Quantity]						DECIMAL (19,4),
	[QuantityIsCommon]				BIT,
	[UnitId]						INT,
	[UnitIsCommon]					BIT,
	[Time1]							DATETIME2 (2),
	[Time1IsCommon]					BIT,
	[Duration]						DECIMAL (19,4),
	[DurationIsCommon]				BIT,	
	[DurationUnitId]				INT,
	[DurationUnitIsCommon]			BIT,
	[Time2]							DATETIME2 (2),
	[Time2IsCommon]					BIT,

	[ExternalReference]				NVARCHAR (50),
	[ExternalReferenceIsCommon]		BIT,
	[ReferenceSourceId]				INT,
	[ReferenceSourceIsCommon]		BIT,
	[InternalReference]				NVARCHAR (50),
	[InternalReferenceIsCommon]		BIT
);