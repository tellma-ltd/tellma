CREATE TABLE [dbo].[AccountTypeNotedRelationDefinitions] (
	[Id]					INT CONSTRAINT [PK_AccountTypeNotedRelationDefinitions] PRIMARY KEY IDENTITY,
	[AccountTypeId]			INT NOT NULL CONSTRAINT [FK_AccountTypeNotedRelationDefinitions__AccountTypeId] REFERENCES dbo.[AccountTypes]([Id]) ON DELETE CASCADE,
	[NotedRelationDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountTypeNotedRelationDefinitions__NotedRelationDefinitionId] REFERENCES dbo.RelationDefinitions([Id]),
	-- Audit details
	[SavedById]			INT				NOT NULL CONSTRAINT [FK_AccountTypeNotedRelationDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[AccountTypeNotedRelationDefinitionsHistory]));
GO