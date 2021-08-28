CREATE TYPE [dbo].[InventoryEntryList] AS TABLE (
	[Index]					INT		PRIMARY KEY			IDENTITY,
	[PostingDate]			DATE,
	[AgentId]				INT,
	[ResourceId]			INT
);