CREATE TABLE [dbo].[LineDefinitionEntryAgentDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryAgentDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryAgentDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[AgentDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryAgentDefinitions__AgentDefinitionId] REFERENCES dbo.[AgentDefinitions]([Id]),
	-- Audit details
	[SavedById]					INT				NOT NULL CONSTRAINT [FK_LineDefinitionEntryAgentDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionEntryAgentDefinitionsHistory]));