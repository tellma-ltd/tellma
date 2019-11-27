CREATE TABLE [dbo].[ResourceTypesEntryTypes] (
	[ResourceTypeId]	NVARCHAR (255)	CONSTRAINT [FK_ResourceTypesEntryTypes__ResourceTypeId] REFERENCES [dbo].[ResourceTypes] ([Id]), 
	[EntryTypeId]		NVARCHAR (255)	CONSTRAINT [FK_ResourceTypesEntryTypes__EntryTypeId] REFERENCES [dbo].[EntryTypes] ([Id]),
	CONSTRAINT [PK_AccountTypesEntryTypes] PRIMARY KEY ([ResourceTypeId], [EntryTypeId])
);
GO;