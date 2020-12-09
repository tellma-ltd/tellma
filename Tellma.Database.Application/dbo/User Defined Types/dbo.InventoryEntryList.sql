CREATE TYPE [dbo].[InventoryEntryList] AS TABLE (
	[Index]					INT		PRIMARY KEY			IDENTITY,
	[ResourceId]			INT,
	[CustodyId]				INT,
	[PostingDate]			NVARCHAR (255)
);