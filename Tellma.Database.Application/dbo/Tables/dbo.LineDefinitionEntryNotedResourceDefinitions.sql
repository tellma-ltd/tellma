CREATE TABLE [dbo].[LineDefinitionEntryNotedResourceDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryNotedResourceDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryNotedResourceDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[NotedResourceDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryNotedResourceDefinitions__NotedResourceDefinitionId] REFERENCES dbo.[ResourceDefinitions]([Id]),
	-- Audit details
	[SavedById]					INT				NOT NULL CONSTRAINT [FK_LineDefinitionEntryNotedResourceDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionEntryNotedResourceDefinitionsHistory]));