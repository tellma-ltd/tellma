CREATE TYPE [dbo].EntryList AS TABLE (
	[Index]						INT					,
	[LineIndex]					INT					INDEX IX_EntryList_LineIndex ([LineIndex]),
	[DocumentIndex]				INT					INDEX IX_EntryList_DocumentIndex ([DocumentIndex]),
	PRIMARY KEY ([Index], [LineIndex], [DocumentIndex]),
	[Id]						INT,
	[Direction]					SMALLINT,
	[AccountId]					INT					INDEX IX_EntryList_AccountId ([AccountId]),		
	[CurrencyId]				NCHAR (3),
	[RelationId]				INT					INDEX IX_Entries__RelationId ([RelationId]),
	[CustodianId]				INT					INDEX IX_EntryList_CustodianId ([CustodianId]),
	[NotedRelationId]			INT					INDEX IX_EntryList_NotedAgentId ([NotedRelationId]),		
	[ResourceId]				INT					INDEX IX_EntryList_ResourceId ([ResourceId]),
	[CenterId]					INT					INDEX IX_EntryList_CenterId ([CenterId]),
	[EntryTypeId]				INT,
	[MonetaryValue]				DECIMAL (19,4),--		NOT NULL DEFAULT 0, -- Amount in foreign Currency 
	[Quantity]					DECIMAL (19,4),
	[UnitId]					INT,
	[Value]						DECIMAL (19,4),--		NOT NULL DEFAULT 0 ,-- equivalent in functional currency
	[RValue]					DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- re-instated in functional currency
	[PValue]					DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- equivalent in presentation currency

	[Time1]						DATETIME2 (2),	-- from time
	[Duration]					DECIMAL (19,4),
	[DurationUnitId]			INT,
	[Time2]						DATETIME2 (2),	-- to time
	[ExternalReference]			NVARCHAR (50),
	[ReferenceSourceId]			INT,
	[InternalReference]			NVARCHAR (50),
	[NotedAgentName]			NVARCHAR (50),
	[NotedAmount]				DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[NotedDate]					DATE
);