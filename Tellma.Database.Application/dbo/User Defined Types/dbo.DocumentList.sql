CREATE TYPE [dbo].[DocumentList] AS TABLE (
	[Index]							INT				PRIMARY KEY,-- IDENTITY (0,1),
	[Id]							INT				NOT NULL DEFAULT 0,
	[SerialNumber]					INT,
	[Clearance]						TINYINT			NOT NULL DEFAULT 0,
	[PostingDate]					DATE,
	[PostingDateIsCommon]			BIT				NOT NULL DEFAULT 1,
	[Memo]							NVARCHAR (255),	
	[MemoIsCommon]					BIT				DEFAULT 0,
	[SegmentId]						INT,
	[CenterId]						INT,
	[CenterIsCommon]				BIT				NOT NULL DEFAULT 0,
	[ParticipantId]					INT,
	[ParticipantIsCommon]			BIT				NOT NULL DEFAULT 0,
	[CurrencyId]					NCHAR (3), 
	[CurrencyIsCommon]				BIT				NOT NULL DEFAULT 0,
	[ExternalReference]				NVARCHAR (50), -- e.g., invoice number
	[ExternalReferenceIsCommon]		BIT				NOT NULL DEFAULT 0,

	[AdditionalReference]			NVARCHAR (50), -- e.g., machine number
	[AdditionalReferenceIsCommon]	BIT				NOT NULL DEFAULT 0,
	
	-- Extra Columns not in Document.cs
	[UpdateAttachments]			BIT
);