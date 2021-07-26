CREATE TABLE [dbo].[LineDefinitionEntryCustodianDefinitions]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionEntryCustodianDefinitions] PRIMARY KEY IDENTITY,
	[LineDefinitionEntryId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryCustodianDefinitions__LineDefinitionEntryId] REFERENCES dbo.[LineDefinitionEntries]([Id]) ON DELETE CASCADE,
	[CustodianDefinitionId]	INT NOT NULL CONSTRAINT [FK_LineDefinitionEntryCustodianDefinitions__CustodianDefinitionId] REFERENCES dbo.[RelationDefinitions]([Id]),
	-- Audit details
	[SavedById]					INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntryCustodianDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionEntryCustodianDefinitionsHistory]));
GO