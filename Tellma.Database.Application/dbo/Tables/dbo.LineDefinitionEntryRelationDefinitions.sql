CREATE TABLE [dbo].[LineDefinitionEntryRelationDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryRelationDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryRelationDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[RelationDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryRelationDefinitions__RelationDefinitionId] REFERENCES dbo.[RelationDefinitions]([Id]),
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryRelationDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryRelationDefinitions__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
);