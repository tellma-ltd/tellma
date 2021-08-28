CREATE TABLE [dbo].[AccountTypeAgentDefinitions] (
	[Id]					INT CONSTRAINT [PK_AccountTypeAgentDefinitions] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_AccountTypeAgentDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[AgentDefinitionId]		INT NOT NULL CONSTRAINT [FK_AccountTypeAgentDefinitions__AgentDefinitionId] REFERENCES dbo.[AgentDefinitions]([Id]),
	-- Audit details
	[SavedById]			INT				NOT NULL CONSTRAINT [FK_AccountTypeAgentDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypeRelationDefinitionsHistory]));
GO