CREATE TYPE [dbo].[DocumentWideLineList] AS TABLE (
	[Index]					INT PRIMARY KEY,
	[DocumentLineIndex]		INT					NOT NULL,
	[DocumentIndex]			INT					NOT NULL,
	[Id]					INT					NOT NULL,
	[DocumentLineId]		INT					NOT NULL,
	[DocumentId]			INT					NOT NULL,
	[LineDefinitionId]			NVARCHAR (255)		NOT NULL,
	[TemplateLineId]		INT,
	[ScalingFactor]			FLOAT,
	
	[Direction1]			SMALLINT			NOT NULL,
	[AccountId1]			INT	NOT NULL,
	[IfrsEntryClassificationId1]			NVARCHAR (255),		-- Note that the responsibility center might define the Ifrs Note
	[ResourceId1]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'FunctionalCurrencyId')),
	[InstanceId1]			INT,
	[BatchCode1]			NVARCHAR (255),
	[DueDate1]				DATE, -- applies to temporary accounts, such as loans and borrowings
	[MoneyAmount1]			MONEY				NOT NULL DEFAULT 0, -- Amount in foreign Currency 
	[Mass1]					DECIMAL				NOT NULL DEFAULT 0, -- MassUnit, like LTZ bar, cement bag, etc
	[Volume1]				DECIMAL				NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping
	[Area1]					DECIMAL				NOT NULL DEFAULT 0, -- Area Unit, possibly for lands
	[Length1]				DECIMAL				NOT NULL DEFAULT 0, -- Length Unit, possibly for cables or pipes
	[Time1]					DECIMAL				NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[Count1]				DECIMAL				NOT NULL DEFAULT 0, -- CountUnit
	[Value1]				VTYPE				NOT NULL DEFAULT 0, -- equivalent in functional currency
	[Memo1]					NVARCHAR (255), -- a textual description for statements and reports
	[ExternalReference1]	NVARCHAR (255),
	[AdditionalReference1]	NVARCHAR (255),
	[RelatedResourceId1]	INT, -- Good, Service, Labor, Machine usage
	[RelatedAgentId1]		INT,
	[RelatedQuantity1]		MONEY ,		-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedMoneyAmount1]	MONEY 				NOT NULL DEFAULT 0,

	[Direction2]			SMALLINT			NOT NULL,
	[AccountId2]			INT					NOT NULL,
	[IfrsEntryClassificationId2]			NVARCHAR (255),		-- Note that the responsibility center might define the Ifrs Note
	[ResourceId2]			INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'FunctionalCurrencyId')),
	[InstanceId2]			INT,
	[BatchCode2]			NVARCHAR (255),
	[DueDate2]				DATE, -- applies to temporary accounts, such as loans and borrowings
	[MoneyAmount2]			MONEY				NOT NULL DEFAULT 0, -- Amount in foreign Currency 
	[Mass2]					DECIMAL				NOT NULL DEFAULT 0, -- MassUnit, like LTZ bar, cement bag, etc
	[Volume2]				DECIMAL				NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping
	[Area2]					DECIMAL				NOT NULL DEFAULT 0, -- Area Unit, possibly for lands
	[Length2]				DECIMAL				NOT NULL DEFAULT 0, -- Length Unit, possibly for cables or pipes
	[Time2]					DECIMAL				NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[Count2]				DECIMAL				NOT NULL DEFAULT 0, -- CountUnit
	[Value2]				VTYPE				NOT NULL DEFAULT 0, -- equivalent in functional currency
	[Memo2]					NVARCHAR (255), -- a textual description for statements and reports
	[ExternalReference2]	NVARCHAR (255),
	[AdditionalReference2]	NVARCHAR (255),
	[RelatedResourceId2]	INT, -- Good, Service, Labor, Machine usage
	[RelatedAgentId2]		INT,
	[RelatedQuantity2]		MONEY ,		-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedMoneyAmount2]	MONEY 				NOT NULL DEFAULT 0
);