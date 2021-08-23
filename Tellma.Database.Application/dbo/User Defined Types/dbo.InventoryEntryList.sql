CREATE TYPE [dbo].[InventoryEntryList] AS TABLE (
	[Index]					INT		PRIMARY KEY			IDENTITY,
	[ResourceId]			INT,
	[RelationId]			INT,
	[PostingDate]			NVARCHAR (255)
);