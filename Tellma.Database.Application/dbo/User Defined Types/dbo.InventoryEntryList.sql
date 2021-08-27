CREATE TYPE [dbo].[InventoryEntryList] AS TABLE (
	[Index]					INT		PRIMARY KEY			IDENTITY,
	-- Weird error. Why the date is NVARCHAR?! Commended 2021.08.25
	--[PostingDate]			NVARCHAR (255),
	[PostingDate]			DATE,
	[RelationId]			INT,
	[ResourceId]			INT
);