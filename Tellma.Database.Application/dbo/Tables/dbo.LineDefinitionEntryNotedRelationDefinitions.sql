CREATE TABLE [dbo].[LineDefinitionEntryNotedRelationDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryNotedRelationDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryNotedRelationDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[NotedRelationDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryNotedRelationDefinitions__NotedRelationDefinitionId] REFERENCES dbo.[RelationDefinitions]([Id]),
	-- Audit details
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryNotedRelationDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryNotedRelationDefinitions__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
);