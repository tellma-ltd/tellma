CREATE TYPE [dbo].DocumentLineEntryList AS TABLE (
	[Index]					INT,
	[DocumentLineIndex]		INT					NOT NULL,
	[DocumentIndex]			INT					NOT NULL,
	[Id]					INT NOT NULL DEFAULT 0,
	[DocumentLineId]		INT,
	[EntryNumber]			INT,
	[Direction]				SMALLINT			NOT NULL,
	[AccountId]				INT		NOT NULL,
	[IfrsNoteId]			NVARCHAR (255),		-- Note that the responsibility center might define the Ifrs Note
	[ResponsibilityCenterId]INT,				-- called SegmentId in B10. When not needed, we use the entity itself.
	[ResourceId]			INT,				-- NUll because it may be specified by Account				
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

	[EntityState]			NVARCHAR (255)		NOT NULL DEFAULT(N'Inserted'),
	PRIMARY KEY ([Index]),
	INDEX IX_TransactionEntryList_TransactionLineIndex ([DocumentLineIndex]),
	CHECK ([Direction] IN (-1, 1)),
	CHECK ([EntityState] IN (N'Unchanged', N'Inserted', N'Updated', N'Deleted'))
);