CREATE TABLE [dbo].[LineDefinitionEntryRelationDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryRelationDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryRelationDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[RelationDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryRelationDefinitions__RelationDefinitionId] REFERENCES dbo.[RelationDefinitions]([Id]),
	-- Audit details
	[SavedById]					INT				NOT NULL CONSTRAINT [FK_LineDefinitionEntryRelationDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionEntryRelationDefinitionsHistory]));