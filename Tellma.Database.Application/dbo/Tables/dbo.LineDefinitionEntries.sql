﻿CREATE TABLE [dbo].[LineDefinitionEntries] (
	[Id]						INT					CONSTRAINT [PK_LineDefinitionEntries] PRIMARY KEY NONCLUSTERED IDENTITY,
	[LineDefinitionId]			INT					NOT NULL CONSTRAINT [FK_LineDefinitionEntries_LineDefinitionId] REFERENCES dbo.[LineDefinitions]([Id]) ON DELETE CASCADE,
	[Index]						INT					NOT NULL CONSTRAINT [CK_LineDefinitionEntries_Index] CHECK([Index] >= 0),
	CONSTRAINT [UQ_LineDefinitionEntries] UNIQUE CLUSTERED ([LineDefinitionId], [Index]),
	[Direction]					SMALLINT			NOT NULL CONSTRAINT [CK_LineDefinitionEntries_Direction] CHECK([Direction] IN (-1, +1)),
	[ParentAccountTypeId]		INT NOT NULL		CONSTRAINT [FK_LineDefinition__ParentAccountTypeId] REFERENCES dbo.AccountTypes([Id]),
	[EntryTypeId]				INT					CONSTRAINT [FK_LineDefinitionEntries__EntryTypeId] REFERENCES [dbo].[EntryTypes] ([Id]),
	[SavedById]					INT					NOT NULL CONSTRAINT [FK_LineDefinitionEntries__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2			GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2			GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionEntriesHistory]));
GO;
CREATE INDEX [IX_LineDefinitionEntries_ParentAccountTypeId] ON [dbo].[LineDefinitionEntries] ([ParentAccountTypeId])
GO;
CREATE INDEX [IX_LineDefinitionEntries_EntryTypeId] ON [dbo].[LineDefinitionEntries] ([EntryTypeId])
GO;
