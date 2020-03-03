CREATE TABLE [dbo].LineDefinitionEntries (
	[Id]						INT					CONSTRAINT [PK_LineDefinitionEntries] PRIMARY KEY NONCLUSTERED IDENTITY,
	[LineDefinitionId]			NVARCHAR (50)		NOT NULL CONSTRAINT [FK_LineDefinitionEntries_LineDefinitions] REFERENCES [dbo].[LineDefinitions] ([Id]),
	[EntryNumber]				INT					NOT NULL  CONSTRAINT [CK_LineDefinitionEntries_EntryNumber]	CHECK([EntryNumber] >= 0),
	CONSTRAINT [IX_LineDefinitionEntries] UNIQUE CLUSTERED ([LineDefinitionId], [EntryNumber]),
	[Direction]					SMALLINT			NOT NULL CHECK([Direction] IN (-1, +1)),
	[AccountTypeParentCode]		NVARCHAR (255)		NOT NULL CONSTRAINT [FK_LineDefinitionEntries__AccountTypeParentCode] REFERENCES dbo.AccountTypes([Code]),
	[AgentDefinitionId]			NVARCHAR (50)		CONSTRAINT [FK_LineDefinitionEntries_AgentDefinitions] REFERENCES [dbo].[AgentDefinitions] ([Id]),
	[EntryTypeCode]				NVARCHAR (255)		CONSTRAINT [FK_LineDefinitionEntries_EntryTypeCode] REFERENCES [dbo].[EntryTypes] ([Code]),
	[SavedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionEntries__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2			GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2			GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionEntriesHistory]));
GO;