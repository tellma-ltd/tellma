CREATE TABLE [dbo].[LineDefinitionEntryAccountTypes]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryAccountTypes] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryAccountTypes__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryAccountTypes__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]),
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryAccountTypes__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryAccountTypes__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
);