CREATE TABLE [dbo].[AccountTypesEntryTypes] (
	[AccountTypeId]	NVARCHAR (50)	CONSTRAINT [FK_AccountTypesEntryTypes__AccountTypeId] FOREIGN KEY ([AccountTypeId]) REFERENCES [dbo].[AccountGroups] ([Id]) ON DELETE CASCADE, 
	[EntryTypeId]	NVARCHAR (255)	CONSTRAINT [FK_AccountTypesEntryTypes__EntryTypeId] FOREIGN KEY ([EntryTypeId]) REFERENCES [dbo].[EntryTypes] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [PK_AccountTypesEntryTypes] PRIMARY KEY ([AccountTypeId], [EntryTypeId])
);
GO;