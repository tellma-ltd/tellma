CREATE TABLE [dbo].[AccountTypeNotedAgentDefinitions] (
	[Id]					INT CONSTRAINT [PK_AccountTypeNotedAgentDefinitions] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_AccountTypeNotedAgentDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[NotedAgentDefinitionId]INT NOT NULL CONSTRAINT FK_AccountTypeNotedAgentDefinitions__NotedAgentDefinitionId REFERENCES dbo.[AgentDefinitions]([Id]),
	-- Audit details
	[SavedById]			INT				NOT NULL CONSTRAINT FK_AccountTypeNotedAgentDefinitions__SavedById REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypeNotedAgentDefinitionsHistory]));
GO