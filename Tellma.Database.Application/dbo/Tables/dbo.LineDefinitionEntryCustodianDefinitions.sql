CREATE TABLE [dbo].[LineDefinitionEntryCustodianDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryCustodianDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryCustodianDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[CustodianDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryCustodianDefinitions__CustodianDefinitionId] REFERENCES dbo.[RelationDefinitions]([Id]),
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryCustodianDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryCustodianDefinitions__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
);