CREATE TABLE [dbo].[LineDefinitionEntryResourceDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryResourceDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryResourceDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[ResourceDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryResourceDefinitions__ResourceDefinitionId] REFERENCES dbo.[ResourceDefinitions]([Id]),
	-- Audit details
	[SavedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryResourceDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]				DATETIME2			GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]				DATETIME2			GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionEntryResourceDefinitionsHistory]));