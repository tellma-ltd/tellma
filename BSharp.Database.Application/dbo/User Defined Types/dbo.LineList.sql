CREATE TYPE [dbo].[LineList] AS TABLE (
	[Index]						INT				PRIMARY KEY,
	[DocumentIndex]				INT				NOT NULL DEFAULT 0,
	[Id]						INT				NOT NULL DEFAULT 0,
	[DefinitionId]				NVARCHAR (50)	NOT NULL,
	[CurrencyId]				NCHAR (3),
	[AgentId]					INT,
	[ResourceId]				INT,
	[Amount]					DECIMAL (19,4),

-- Additional information to satisfy reporting requirements
-- While Voucher Number referes to the source document, this refers to any other identifying string 
-- for support documents, such as deposit slip reference, invoice number, etc...
	[Memo]						NVARCHAR (255), -- a textual description for statements and reports
	[ExternalReference]			NVARCHAR (50),
	[AdditionalReference]		NVARCHAR (50)

	--[RelatedResourceId]		INT, -- Good, Service, Labor, Machine usage
	--[RelatedAgentId]		INT,
	--[RelatedQuantity]		DECIMAL (19,4) ,			-- used in Tax accounts, to store the quantiy of taxable item
	--[RelatedDECIMAL (19,4)Amount]	DECIMAL (19,4) 				NOT NULL DEFAULT 0 -- e.g., amount subject to tax
);