CREATE TYPE [dbo].[DocumentList] AS TABLE (
	[Index]							INT				PRIMARY KEY,-- IDENTITY (0,1),
	[Id]							INT				NOT NULL DEFAULT 0,
	[SerialNumber]					INT,
	[DocumentDate]					DATE			NOT NULL DEFAULT (CONVERT (date, SYSDATETIME())),
	[Clearance]						TINYINT			NOT NULL DEFAULT 0,
	[DocumentLookup1Id]				INT, -- e.g., cash machine serial in the case of a sale
	[DocumentLookup2Id]				INT,
	[DocumentLookup3Id]				INT,
	[DocumentText1]					NVARCHAR (255),
	[DocumentText2]					NVARCHAR (255),
	[Memo]							NVARCHAR (255),	
	[MemoIsCommon]					BIT				DEFAULT 1,
	[AgentId]						INT,
	[AgentIdIsCommon]				BIT				NOT NULL DEFAULT 1,
	[InvestmentCenterId]			INT,
	[InvestmentCenterIsCommon]		BIT				NOT NULL DEFAULT 1,
	[Time1]							DATETIME2 (2),
	[Time1IsCommon]					BIT				NOT NULL DEFAULT 1,
	[Time2]							DATETIME2 (2), -- must be less than DocumentDate when posting
	[Time2IsCommon]					BIT				NOT NULL DEFAULT 1,
	[Quantity]						DECIMAL (19,4)	NULL,
	[QuantityIsCommon]				BIT				NOT NULL DEFAULT 1,
	[UnitId]						INT,
	[UnitIsCommon]					BIT				NOT NULL DEFAULT 1
	--[CurrencyId]							INT, 
	--[CurrencyIsCommon]						BIT				DEFAULT 1,
	--[InvoiceReference]						NVARCHAR (255),
	--[InvoiceReferenceIsCommon]				BIT				DEFAULT 1,

	--[Frequency]			NVARCHAR (30)		NOT NULL DEFAULT (N'OneTime'), -- an easy way to define a recurrent document
	--[Repetitions]		INT					NOT NULL DEFAULT 0, -- time unit is function of frequency

	--CHECK ([Frequency] IN (N'OneTime', N'Daily', N'Weekly', N'Monthly', N'Quarterly', N'Yearly'))
);