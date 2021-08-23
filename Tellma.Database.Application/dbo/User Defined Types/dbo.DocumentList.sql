CREATE TYPE [dbo].[DocumentList] AS TABLE (
	[Index]							INT				PRIMARY KEY,-- IDENTITY (0,1),
	[Id]							INT				NOT NULL DEFAULT 0,
	[SerialNumber]					INT,
	[Clearance]						TINYINT,

	[PostingDate]					DATE,
	[PostingDateIsCommon]			BIT,
	[Memo]							NVARCHAR (255),	
	[MemoIsCommon]					BIT,
	
	[CurrencyId]					NCHAR (3), 
	[CurrencyIsCommon]				BIT,
	[CenterId]						INT,
	[CenterIsCommon]				BIT,
	
	[RelationId]					INT,
	[RelationIsCommon]				BIT,

	[NotedRelationId]				INT,
	[NotedRelationIsCommon]			BIT,
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

	[ExternalReference]				NVARCHAR (50), -- e.g., invoice number
	[ExternalReferenceIsCommon]		BIT,
	[ReferenceSourceId]				INT,
	[ReferenceSourceIsCommon]		BIT,
	[InternalReference]				NVARCHAR (50), -- e.g., machine number
	[InternalReferenceIsCommon]		BIT,
	
	-- Extra Columns not in Document.cs
	[UpdateAttachments]				BIT
);