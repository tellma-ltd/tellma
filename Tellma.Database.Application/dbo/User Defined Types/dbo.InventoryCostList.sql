CREATE TYPE [dbo].[InventoryCostList] AS TABLE (
	[Index]					INT		PRIMARY KEY			IDENTITY,
	[ResourceId]			INT,
	[CustodyId]				INT,
	[PostingDate]			NVARCHAR (255),
	[NetMonetaryValue]		DECIMAL (19,4) DEFAULT (0),
	[NetValue]				DECIMAL (19,4) DEFAULT (0),
	[NetQuantity]			DECIMAL (19,4) DEFAULT (0),
	INDEX IX_InventoryCostList_ResourceId_CustodyId_PostingDate ([ResourceId], [CustodyId], [PostingDate])
);