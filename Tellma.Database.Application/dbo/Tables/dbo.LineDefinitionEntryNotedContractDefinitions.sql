CREATE TABLE [dbo].[LineDefinitionEntryNoteContractDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryNoteContractDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryNoteContractDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[NoteContractDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryNoteContractDefinitions__NoteContractDefinitionId] REFERENCES dbo.[ContractDefinitions]([Id]),
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryNoteContractDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryNoteContractDefinitions__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
);