CREATE TYPE [dbo].[DocumentList] AS TABLE (
	[Index]							INT				PRIMARY KEY,-- IDENTITY (0,1),
	[Id]							INT				NOT NULL DEFAULT 0,
	[SerialNumber]					INT,
	[Clearance]						TINYINT			NOT NULL DEFAULT 0,

	[PostingDate]					DATE,
	[PostingDateIsCommon]			BIT				NOT NULL DEFAULT 1,
	[Memo]							NVARCHAR (255),	
	[MemoIsCommon]					BIT				DEFAULT 0,
	
	[CurrencyId]					NCHAR (3), 
	[CurrencyIsCommon]				BIT				NOT NULL DEFAULT 0,
	[CenterId]						INT,
	[CenterIsCommon]				BIT				NOT NULL DEFAULT 0,
	
	[RelationId]					INT,
	[RelationIsCommon]				BIT				NOT NULL DEFAULT 0,
	[CustodianId]					INT,
	[CustodianIsCommon]				BIT				NOT NULL DEFAULT 0,

	[NotedRelationId]				INT,
	[NotedRelationIsCommon]			BIT				NOT NULL DEFAULT 0,
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

	[ExternalReference]				NVARCHAR (50), -- e.g., invoice number
	[ExternalReferenceIsCommon]		BIT				NOT NULL DEFAULT 0,
	[ReferenceSourceId]				INT,
	[ReferenceSourceIsCommon]		BIT				NOT NULL DEFAULT 0,
	[InternalReference]				NVARCHAR (50), -- e.g., machine number
	[InternalReferenceIsCommon]		BIT				NOT NULL DEFAULT 0,
	
	-- Extra Columns not in Document.cs
	[UpdateAttachments]				BIT
);