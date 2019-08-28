CREATE TYPE [dbo].DocumentLineEntryList AS TABLE (
	[Index]					INT					PRIMARY KEY IDENTITY (0,1),
	[DocumentLineIndex]		INT					NOT NULL DEFAULT 0,
	[DocumentIndex]			INT					NOT NULL DEFAULT 0,
	[Id]					INT					NOT NULL DEFAULT 0,
	[DocumentLineId]		INT					NOT NULL DEFAULT 0,
	[EntryNumber]			INT					NOT NULL DEFAULT 1,
	[Direction]				SMALLINT			NOT NULL,
	[AccountId]				INT		NOT NULL,
	[IfrsNoteId]			NVARCHAR (255),		-- Note that the responsibility center might define the Ifrs Note
	[ResponsibilityCenterId]INT,				-- called SegmentId in B10. When not needed, we use the entity itself.
	[ResourceId]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'FunctionalCurrencyId')), -- because it may be specified by Account				
	[InstanceId]			INT,
	[BatchCode]				NVARCHAR (255),
	[DueDate]				DATE,
	[Quantity]				VTYPE				NOT NULL DEFAULT 0, --  measure on which the value is based. If it is MassMeasure then [Mass] must equal [ValueMeasure] and so on.
	[MoneyAmount]			MONEY				NOT NULL DEFAULT 0, -- Amount in foreign Currency 
	[Mass]					DECIMAL				NOT NULL DEFAULT 0, -- MassUnit, like LTZ bar, cement bag, etc
	[Volume]				DECIMAL				NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping
	[Area]					DECIMAL				NOT NULL DEFAULT 0, -- Area Unit, possibly for lands
	[Length]				DECIMAL				NOT NULL DEFAULT 0, 
	[Time]					DECIMAL				NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[Count]					DECIMAL				NOT NULL DEFAULT 0, -- CountUnit
	[Value]					VTYPE				NOT NULL DEFAULT 0, -- equivalent in functional currency
	[Memo]					NVARCHAR (255), -- a textual description for statements and reports
	[ExternalReference]		NVARCHAR (255),
	[AdditionalReference]	NVARCHAR (255),

	[RelatedResourceId]		INT, -- Good, Service, Labor, Machine usage
	[RelatedAgentId]		INT,
	[RelatedQuantity]		MONEY ,			-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedMoneyAmount]	MONEY 				NOT NULL DEFAULT 0, -- e.g., amount subject to tax
	[SortKey]				DECIMAL (9,4)

	INDEX IX_DocumentEntryList_DocumentLineIndex ([DocumentLineIndex]),
	CHECK ([Direction] IN (-1, 1))
);