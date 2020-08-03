CREATE TABLE [dbo].[LineDefinitionEntryCustodyDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryCustodyDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryCustodyDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[CustodyDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryCustodyDefinitions__CustodyDefinitionId] REFERENCES dbo.[CustodyDefinitions]([Id]),
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryCustodyDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryCustodyDefinitions__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
);