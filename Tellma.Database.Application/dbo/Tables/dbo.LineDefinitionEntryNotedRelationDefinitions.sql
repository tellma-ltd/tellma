CREATE TABLE [dbo].[LineDefinitionEntryNotedRelationDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryNotedRelationDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryNotedRelationDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[NotedRelationDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryNotedRelationDefinitions__NotedRelationDefinitionId] REFERENCES dbo.[RelationDefinitions]([Id]),
	-- Audit details
	[SavedById]					INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryNotedRelationDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionEntryNotedRelationDefinitionsHistory]));