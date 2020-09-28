CREATE TYPE [dbo].EntryList AS TABLE (
	[Index]						INT					,
	[LineIndex]					INT					INDEX IX_EntryList_LineIndex ([LineIndex]),
	[DocumentIndex]				INT					INDEX IX_EntryList_DocumentIndex ([DocumentIndex]),
	PRIMARY KEY ([Index], [LineIndex], [DocumentIndex]),
	[Id]						INT					NOT NULL DEFAULT 0,
	[IsSystem]					BIT					NOT NULL DEFAULT 0,
	[Direction]					SMALLINT,
	[AccountId]					INT,
	[CurrencyId]				NCHAR (3),
	[CustodianId]				INT,
	[CustodyId]					INT,
	[ParticipantId]				INT,
	[ResourceId]				INT,
	[CenterId]					INT,
	[EntryTypeId]				INT,
	[MonetaryValue]				DECIMAL (19,4),--		NOT NULL DEFAULT 0, -- Amount in foreign Currency 
	[Quantity]					DECIMAL (19,4),
	[UnitId]					INT,
	[Value]						DECIMAL (19,4),--		NOT NULL DEFAULT 0 ,-- equivalent in functional currency

	[Time1]						DATETIME2 (2),	-- from time
	[Time2]						DATETIME2 (2),	-- to time
	[ExternalReference]			NVARCHAR (50),
	[AdditionalReference]		NVARCHAR (50),
	[NotedAgentName]			NVARCHAR (50),
	[NotedAmount]				DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[NotedDate]					DATE
);