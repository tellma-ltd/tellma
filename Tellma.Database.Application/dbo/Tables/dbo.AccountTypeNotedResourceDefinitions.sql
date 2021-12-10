CREATE TABLE [dbo].[AccountTypeNotedResourceDefinitions] (
	[Id]					INT CONSTRAINT [PK_AccountTypeNotedResourceDefinitions] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_AccountTypeNotedResourceDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[NotedResourceDefinitionId]INT NOT NULL CONSTRAINT FK_AccountTypeNotedResourceDefinitions__NotedResourceDefinitionId REFERENCES dbo.[ResourceDefinitions]([Id]),
	-- Audit details
	[SavedById]			INT				NOT NULL CONSTRAINT FK_AccountTypeNotedResourceDefinitions__SavedById REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypeNotedResourceDefinitionsHistory]));
GO